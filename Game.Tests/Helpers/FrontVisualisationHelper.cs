using Game.GeneticAlgorithm;
using Game.Screens;
using System.Collections.Generic;
using System.Threading;
using SFML.Graphics;
using SFML.Window;

namespace Game.Tests.Helpers
{
    public static class FrontVisualisationHelper
    {
        public static void ShowPopulation(List<Individual> population)
        {
            // Create the main window
            var window = new RenderWindow(
                new VideoMode(1920/2, 1080/2),
                "Population Visualisation",
                Styles.Titlebar,
                new ContextSettings() { AntialiasingLevel = 8 });

            // Set our frame rate to 60fps so the screen is responsive.
            window.SetFramerateLimit(60);

            // Handle window events
            window.Closed += (sender, b) => ((RenderWindow)sender).Close(); 

            var paretoScreen = new ParetoVisualScreen(window, Configuration.SinglePlayer, population); 
            
            paretoScreen.Camera.SetCentre(new SFML.System.Vector2f(1880, 950));
            paretoScreen.Camera.GetView().Zoom(0.5f);

            paretoScreen.Update(2f);
            window.SetView(paretoScreen.Camera.GetView());

            while (!Keyboard.IsKeyPressed(Keyboard.Key.Escape) 
                && !Keyboard.IsKeyPressed(Configuration.QuitKey)
                && window.IsOpen)
            {
                // Clear the previous frame
                window.Clear(Configuration.Background);

                // Process events
                window.DispatchEvents();

                paretoScreen.Update(0.16f);
                paretoScreen.Camera.Update(0.05f);
                paretoScreen.Draw(0.16f);
                window.Display();

                Thread.Sleep(16);
            }
        }
    }
}

