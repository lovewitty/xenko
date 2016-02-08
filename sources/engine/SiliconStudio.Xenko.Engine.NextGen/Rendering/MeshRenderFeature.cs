using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Renders <see cref="RenderMesh"/>.
    /// </summary>
    public class MeshRenderFeature : RootEffectRenderFeature
    {
        public List<SubRenderFeature> RenderFeatures = new List<SubRenderFeature>();

        /// <inheritdoc/>
        public override bool SupportsRenderObject(RenderObject renderObject)
        {
            return renderObject is RenderMesh;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.AttachRootRenderFeature(this);
                renderFeature.Initialize();
            }
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Extract();
            }
        }

        /// <inheritdoc/>
        public override void PrepareEffectPermutationsImpl()
        {
            base.PrepareEffectPermutationsImpl();

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.PrepareEffectPermutations();
            }
        }

        /// <param name="context"></param>
        /// <inheritdoc/>
        public override void Prepare(RenderContext context)
        {
            base.Prepare(context);

            // Prepare each sub render feature
            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Prepare(context);
            }
        }

        protected override void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
            var renderMesh = (RenderMesh)renderObject;
            var drawData = renderMesh.Mesh.Draw;

            pipelineState.InputElements = drawData.VertexBuffers.CreateInputElements();
            pipelineState.PrimitiveType = drawData.PrimitiveType;
        }

        /// <inheritdoc/>
        public override void Draw(RenderDrawContext context, RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            var commandList = context.CommandList;

            foreach (var renderFeature in RenderFeatures)
            {
                renderFeature.Draw(context, renderView, renderViewStage, startIndex, endIndex);
            }

            var descriptorSets = new DescriptorSet[EffectDescriptorSetSlotCount];

            MeshDraw currentDrawData = null;
            for (int index = startIndex; index < endIndex; index++)
            {
                var renderNodeReference = renderViewStage.RenderNodes[index].RenderNode;
                var renderNode = GetRenderNode(renderNodeReference);

                var renderMesh = (RenderMesh)renderNode.RenderObject;
                var drawData = renderMesh.Mesh.Draw;

                // Get effect
                // TODO: Use real effect slot
                var renderEffect = renderNode.RenderEffect;

                // Bind VB
                if (currentDrawData != drawData)
                {
                    for (int i = 0; i < drawData.VertexBuffers.Length; i++)
                    {
                        var vertexBuffer = drawData.VertexBuffers[i];
                        commandList.SetVertexBuffer(i, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Stride);
                    }
                    if (drawData.IndexBuffer != null)
                        commandList.SetIndexBuffer(drawData.IndexBuffer.Buffer, drawData.IndexBuffer.Offset, drawData.IndexBuffer.Is32Bit);
                    currentDrawData = drawData;
                }

                var resourceGroupOffset = ComputeResourceGroupOffset(renderNodeReference);
                
                // Update cbuffer
                renderEffect.Reflection.BufferUploader.Apply(context.CommandList, ResourceGroupPool, resourceGroupOffset);

                // Bind descriptor sets
                for (int i = 0; i < descriptorSets.Length; ++i)
                {
                    var resourceGroup = ResourceGroupPool[resourceGroupOffset++];
                    if (resourceGroup != null)
                        descriptorSets[i] = resourceGroup.DescriptorSet;
                }

                commandList.SetPipelineState(renderEffect.PipelineState);
                commandList.SetDescriptorSets(0, descriptorSets);

                // Draw
                if (drawData.IndexBuffer == null)
                {
                    commandList.Draw(drawData.DrawCount, drawData.StartLocation);
                }
                else
                {
                    commandList.DrawIndexed(drawData.DrawCount, drawData.StartLocation);
                }
            }
        }
    }
}