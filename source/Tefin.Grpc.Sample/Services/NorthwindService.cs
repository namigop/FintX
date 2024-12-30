using System.Collections.Concurrent;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Northwind;

using static Northwind.NorthwindService;

namespace Tefin.Grpc.Sample.Services;

public class NorthwindService2 : NorthwindServiceBase
{
    
    private readonly ILogger<NorthwindService2> _logger;
    private readonly ConcurrentDictionary<int, Order> _orders;
    private readonly ConcurrentDictionary<int, Customer> _customers;

    public NorthwindService2(ILogger<NorthwindService2> logger)
    {
        this._logger = logger;
        this._orders = new ConcurrentDictionary<int, Order>();
        this._customers = new ConcurrentDictionary<int, Customer>();

        foreach (var i in Enumerable.Range(0, 50)) {
            this._orders.TryAdd(i, new Order() { OrderId = i, CustomerId = i});
        }

        foreach (var i in Enumerable.Range(0, 50)) {
            this._customers.TryAdd(i, new Customer() { CustomerId = i });
        }
    }

    public override Task<Order> GetOrderById(OrderRequest request, ServerCallContext context) {
        this._logger.LogInformation($"Getting order by ID: {request.OrderId}");

        if (this._orders.TryGetValue(request.OrderId, out var order)) {
            return Task.FromResult(order);
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.OrderId} not found"));
    }

    public override async Task GetOrdersByCustomer(
        CustomerRequest request,
        IServerStreamWriter<Order> responseStream,
        ServerCallContext context) {
        this._logger.LogInformation($"Getting orders for customer: {request.CustomerId}");

        var customerOrders = this._orders.Values
            .Where(o => o.CustomerId == request.CustomerId);

        foreach (var order in customerOrders) {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            await responseStream.WriteAsync(order);
        }
    }

    public override Task<CustomersResponse> GetCustomers(CustomersRequest request, ServerCallContext context) {
        this._logger.LogInformation("Getting customers list");

        var pageSize = request.PageSize != 0 ? request.PageSize : 10;
        var pageNumber = request.PageNumber != 0 ? request.PageNumber : 1;

        var customers = this._customers.Values
            .Skip((int)((pageNumber - 1) * pageSize))
            .Take((int)pageSize)
            .ToList();

        var response = new CustomersResponse {
            TotalCount = this._customers.Count
        };
        response.Customers.AddRange(customers);

        return Task.FromResult(response);
    }

    public override async Task UpdateOrderStatus(
        IAsyncStreamReader<OrderStatusUpdate> requestStream,
        IServerStreamWriter<OrderStatusResponse> responseStream,
        ServerCallContext context) {
        await foreach (var update in requestStream.ReadAllAsync()) {
            this._logger.LogInformation($"Updating status for order {update.OrderId} to {update.Status}");

            var response = new OrderStatusResponse {
                OrderId = update.OrderId,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            if (!this._orders.TryGetValue(update.OrderId, out var order)) {
                response.Success = false;
                response.Message = $"Order {update.OrderId} not found";
            }
            else {
                order.Status = update.Status;
                response.Success = true;
                response.Message = $"Status updated to {update.Status}";
            }

            await responseStream.WriteAsync(response);
        }
    }

    public override async Task<OrderDetailsResponse> AddOrderDetails(
        IAsyncStreamReader<OrderDetail> requestStream,
        ServerCallContext context) {
        var processedCount = 0;
        var errors = new List<string>();

        await foreach (var detail in requestStream.ReadAllAsync()) {
            if (!this._orders.TryGetValue(detail.OrderId, out var order)) {
                errors.Add($"Order {detail.OrderId} not found");
                continue;
            }

            order.OrderDetails.Add(detail);
            processedCount++;
        }

        return new OrderDetailsResponse {
            Success = errors.Count == 0,
            ProcessedCount = processedCount,
            Errors = { errors }
        };
    }

    public override Task<SubmitOrderResponse> SubmitOrder(SubmitOrderRequest request, ServerCallContext context) {
        this._logger.LogInformation($"Submitting new order for customer: {request.CustomerId}");

        var newOrderId = this._orders.Count + 1;
        var order = new Order {
            OrderId = newOrderId,
            CustomerId = request.CustomerId,
            EmployeeId = request.EmployeeId,
            OrderDate = Timestamp.FromDateTime(DateTime.UtcNow),
            RequiredDate = request.RequiredDate,
            ShipVia = request.ShipVia,
            Freight = request.Freight,
            ShipName = request.ShipName,
            ShipAddress = request.ShipAddress,
            ShipCity = request.ShipCity,
            ShipRegion = request.ShipRegion,
            ShipPostalCode = request.ShipPostalCode,
            ShipCountry = request.ShipCountry,
            Status = "Pending"
        };
        order.OrderDetails.AddRange(request.OrderDetails);

        if (this._orders.TryAdd(newOrderId, order)) {
            return Task.FromResult(new SubmitOrderResponse {
                Success = true,
                Message = "Order created successfully",
                OrderId = newOrderId,
                CreatedOrder = order
            });
        }

        throw new RpcException(new Status(StatusCode.Internal, "Failed to create order"));
    }
}