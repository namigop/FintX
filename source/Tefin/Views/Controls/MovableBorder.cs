namespace Tefin.Views.Controls; 



using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

public class MovableBorder : Border
    {
        private bool _isPressed; 
        private Point _positionInBlock;
        private TranslateTransform _transform = null!;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            _isPressed = true;
            _positionInBlock = e.GetPosition(Parent as Visual);
            
            if (_transform != null!) 
                _positionInBlock = new Point(
                    _positionInBlock.X - _transform.X,
                    _positionInBlock.Y - _transform.Y);
            
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            _isPressed = false;
            
            base.OnPointerReleased(e);
        }

        
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (!_isPressed)
                return;
            
            if (Parent == null)
                return;

            var viz = (Parent as Visual);
            var currentPosition = e.GetPosition(viz);

            var withinX = (currentPosition.X > 1 && currentPosition.X < viz.Bounds.Width);
            var withinY = currentPosition.Y > 0 && currentPosition.Y < viz.Bounds.Height;
            if (!withinX || !withinY) {
                return;
            }
                
            
            var offsetX = currentPosition.X -  _positionInBlock.X;
            var offsetY = currentPosition.Y - _positionInBlock.Y;

            //Console.WriteLine($"{currentPosition.X} , {currentPosition.Y}");
            _transform = new TranslateTransform(offsetX, offsetY);
            RenderTransform = _transform;
            
            base.OnPointerMoved(e);
        }
    }
