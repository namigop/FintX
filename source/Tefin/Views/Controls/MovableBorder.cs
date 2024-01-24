using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Tefin.Views.Controls;

public class MovableBorder : Border {
    private bool _isPressed;
    private Point _positionInBlock;
    private TranslateTransform _transform = null!;

    protected override void OnPointerMoved(PointerEventArgs e) {
        if (!this._isPressed)
            return;

        if (this.Parent == null)
            return;

        var viz = this.Parent as Visual;
        var currentPosition = e.GetPosition(viz);

        var withinX = currentPosition.X > 1 && currentPosition.X < viz!.Bounds.Width;
        var withinY = currentPosition.Y > 0 && currentPosition.Y < viz!.Bounds.Height;
        if (!withinX || !withinY) {
            return;
        }

        var offsetX = currentPosition.X - this._positionInBlock.X;
        var offsetY = currentPosition.Y - this._positionInBlock.Y;

        //Console.WriteLine($"{currentPosition.X} , {currentPosition.Y}");
        this._transform = new TranslateTransform(offsetX, offsetY);
        this.RenderTransform = this._transform;

        base.OnPointerMoved(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        this._isPressed = true;
        this._positionInBlock = e.GetPosition(this.Parent as Visual);

        if (this._transform != null!)
            this._positionInBlock = new Point(this._positionInBlock.X - this._transform.X, this._positionInBlock.Y - this._transform.Y);

        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        this._isPressed = false;

        base.OnPointerReleased(e);
    }
}