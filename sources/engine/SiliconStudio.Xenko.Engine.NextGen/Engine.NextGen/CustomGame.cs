using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Colors;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine.NextGen
{
    public class CustomGame : Game
    {
        private TestCamera camera;
        protected Scene Scene;
        protected Entity Camera = new Entity();
        private SceneGraphicsCompositorLayers graphicsCompositor;

        private Model model;
        private Material material1, material2;
        private NextGenRenderer nextGenRenderer;

        const bool NewSystem = true;

        protected CameraComponent CameraComponent
        {
            get { return Camera.Get<CameraComponent>(); }
            set
            {
                Camera.Add(value);
                graphicsCompositor.Cameras[0] = value;
            }
        }

        protected override Task LoadContent()
        {
            //Profiler.Enable(GameProfilingKeys.GameDrawFPS);
            ProfilerSystem.EnableProfiling(false, GameProfilingKeys.GameDrawFPS);

            model = Asset.Load<Model>("Model");
            material1 = Asset.Load<Material>("Material1");
            material2 = Asset.Load<Material>("Material2");

            SetupScene();

            int cubeWidth = 16;
            
            for (int i = 0; i < cubeWidth; ++i)
            {
                for (int j = 0; j < cubeWidth; ++j)
                {
                    for (int k = 0; k < cubeWidth; ++k)
                    {
                        var position = new Vector3((i - cubeWidth / 2) * 1.4f, (j - cubeWidth / 2) * 1.4f, (k - cubeWidth / 2) * 1.4f);
                        var material = (k/4)%2 == 0 ? material1 : material2;

                        var entity = new Entity
                        {
                            new ModelComponent { Model = model, Materials = { material } },
                        };
                        entity.Transform.Position = position;
                        Scene.Entities.Add(entity);
                    }
                }
            }

            var spriteSheet = Asset.Load<SpriteSheet>("SpriteSheet");
            var spriteEntity = new Entity
            {
                new SpriteComponent
                {
                    SpriteProvider = new SpriteFromSheet { Sheet = spriteSheet },
                }
            };
            Scene.Entities.Add(spriteEntity);

            camera.Position = new Vector3(35.0f, 5.5f, 22.0f);
            camera.SetTarget(Scene.Entities.Last(), true);

            return base.LoadContent();
        }

        private void SetupScene()
        {
            graphicsCompositor = new SceneGraphicsCompositorLayers
            {
                Cameras = { Camera.Get<CameraComponent>() },
                Master =
                {
                    Renderers =
                    {
                        new ClearRenderFrameRenderer { Color = Color.Green, Name = "Clear frame" },
                        //new SceneCameraRenderer { Mode = new CameraRendererModeForward { Name = "Camera renderer", ModelEffect = "XenkoForwardShadingEffect" } },
                        new SceneCameraRenderer { Mode = nextGenRenderer = new NextGenRenderer { Name = "Camera renderer", ModelEffect = "TestEffect" } },
                    }
                }
            };

            Scene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };
            Scene.Entities.Add(Camera);

            //var ambientLight = new Entity { new LightComponent { Type = new LightAmbient { Color = new ColorRgbProvider(Color.White) }, Intensity = 1 } };
            ////var ambientLight = new Entity { new LightComponent { Type = new LightDirectional { Color = new ColorRgbProvider(Color.White) }, Intensity = 1 } };
            ////ambientLight.Transform.RotationEulerXYZ = new Vector3(0.0f, (float) Math.PI, 0.0f);
            //Scene.Entities.Add(ambientLight);

            var directionalLight = new Entity { new LightComponent { Type = new LightDirectional { Color = new ColorRgbProvider(Color.White), Shadow = { Enabled = true } }, Intensity = 1 }, };
            Scene.Entities.Add(directionalLight);

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);

            camera = new TestCamera();
            CameraComponent = camera.Camera;
            Script.Add(camera);
        }
    }
}