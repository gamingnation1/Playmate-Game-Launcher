using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Game_Launcher_V2.Scripts
{
    internal class Animate
    {
        public static Dictionary<Image, bool> _imageBlurState = new Dictionary<Image, bool>();
        public static Dictionary<DockPanel, bool> _dockPanelOpacityState = new Dictionary<DockPanel, bool>();

        public static void AnimateBlur(Image image)
        {
            // Check if the image is already blurred
            if (_imageBlurState[image])
            {
                // Create a DoubleAnimation to animate the Radius property of the BlurEffect from its current value to 0
                var animation = new DoubleAnimation()
                {
                    From = ((BlurEffect)image.Effect).Radius,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                };

                // Set the target property to the Radius property of the BlurEffect
                Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.Effect).(BlurEffect.Radius)"));

                // Set the frame rate of the animation to 60 frames per second
                Timeline.SetDesiredFrameRate(animation, 60);

                // Create a Storyboard and add the animation to it
                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);

                // Start the animation
                storyboard.Begin(image);

                // Update the blur state for the current image
                _imageBlurState[image] = false;
            }
            else
            {
                // Create a BlurEffect
                var blurEffect = new BlurEffect()
                {
                    Radius = 0,
                    KernelType = KernelType.Gaussian
                };

                // Apply the BlurEffect to the image
                image.Effect = blurEffect;

                // Create a DoubleAnimation to animate the Radius property of the BlurEffect
                var animation = new DoubleAnimation()
                {
                    From = 0,
                    To = 60,
                    Duration = new Duration(TimeSpan.FromSeconds(0.4))
                };

                // Set the frame rate of the animation to 60 frames per second
                Timeline.SetDesiredFrameRate(animation, 60);

                // Set the target property to the Radius property of the BlurEffect
                Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.Effect).(BlurEffect.Radius)"));

                // Create a Storyboard and add the animation to it
                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);

                // Start the animation
                storyboard.Begin(image);

                // Update the blur state for the current image
                _imageBlurState[image] = true;
            }
        }

        public static void AnimateDockPanelOpacity(DockPanel dockPanel)
        {
            // Check if the DockPanel is currently visible
            if (_dockPanelOpacityState[dockPanel])
            {
                // Create a DoubleAnimation to animate the Opacity property of the DockPanel from its current value to 0
                var animation = new DoubleAnimation()
                {
                    From = dockPanel.Opacity,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };

                // Set the target property to the Opacity property of the DockPanel
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

                // Set the frame rate of the animation to 60 frames per second
                Timeline.SetDesiredFrameRate(animation, 60);

                // Create a Storyboard and add the animation to it
                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);

                // Start the animation
                storyboard.Begin(dockPanel);

                // Update the opacity state for the current DockPanel
                _dockPanelOpacityState[dockPanel] = false;
            }
            else
            {
                // Create a DoubleAnimation to animate the Opacity property of the DockPanel from 0 to 1
                var animation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.4))
                };

                // Set the frame rate of the animation to 60 frames per second
                Timeline.SetDesiredFrameRate(animation, 60);

                // Set the target property to the Opacity property of the DockPanel
                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

                // Create a Storyboard and add the animation to it
                var storyboard = new Storyboard();
                storyboard.Children.Add(animation);

                // Start the animation
                storyboard.Begin(dockPanel);

                // Update the opacity state for the current DockPanel
                _dockPanelOpacityState[dockPanel] = true;
            }
        }
    }
}
