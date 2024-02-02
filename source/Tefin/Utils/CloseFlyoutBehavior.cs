using System.Windows.Input;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tefin.Utils;

public class CloseFlyoutBehavior : AvaloniaObject {
    static CloseFlyoutBehavior() {
        CloseOnClickProperty.Changed.AddClassHandler<Interactive>(HandleCloseChanged);
    }

    public static readonly AttachedProperty<bool> CloseOnClickProperty =
        AvaloniaProperty.RegisterAttached<CloseFlyoutBehavior, Interactive, bool>(
            "CloseOnClick", false, false, BindingMode.OneTime);

    public static readonly AttachedProperty<FlyoutBase?> FlyoutProperty =
        AvaloniaProperty.RegisterAttached<CloseFlyoutBehavior, Interactive, FlyoutBase?>(
            "Flyout");


    private static void HandleCloseChanged(Interactive interactElem, AvaloniaPropertyChangedEventArgs args) {
        
        static bool HasContextFlyout(Control c) {
            var prop = c.GetType().GetProperty("ContextFlyout");
            if (prop != null) {
                var fly = prop.GetValue(c);
                return (fly != null);
            }

            return false;
        }
        
        static bool HasFlyout(Control c) {
            var prop = c.GetType().GetProperty("Flyout");
            if (prop != null) {
                var fly = prop.GetValue(c);
                return (fly != null);
            }

            return false;
        }
        
        void Handler(object s, RoutedEventArgs e) {
            if (s is Interactive interactElem) {
                bool close = interactElem.GetValue(CloseOnClickProperty);
                if (close) {
                    var flyout = interactElem.GetValue(FlyoutProperty);
                    if (interactElem is StyledElement elem) {
                        if (flyout == null) {
                            var tg = elem.FindParent<Control>(HasContextFlyout);
                            if (tg != null) {
                                flyout = tg.ContextFlyout;
                            }
                        }
                        if (flyout == null) {
                            var tg = elem.FindParent<ContentControl>(HasFlyout);
                            if (tg != null) {
                                var i = tg.GetType().GetProperty("Flyout")!.GetValue(tg);
                                flyout = i as FlyoutBase;
                            }
                        }
                    }
                    
                    flyout?.Hide();
                }


            }
        }

        if (args.NewValue is bool val) {
            interactElem.AddHandler(InputElement.TappedEvent, Handler!);
        }
        else {
            interactElem.RemoveHandler(InputElement.TappedEvent, Handler!);
        }
    }

    public static void SetCloseOnClick(AvaloniaObject element, bool close) {
        element.SetValue(CloseOnClickProperty, close);
    }

    public static bool GetCloseOnClick(AvaloniaObject element) {
        return element.GetValue(CloseOnClickProperty);
    }

    public static void SetFlyout(AvaloniaObject element, bool close) {
        element.SetValue(FlyoutProperty, close);
    }

    public static FlyoutBase? GetFlyout(AvaloniaObject element) {
        return element.GetValue(FlyoutProperty);
    }
}