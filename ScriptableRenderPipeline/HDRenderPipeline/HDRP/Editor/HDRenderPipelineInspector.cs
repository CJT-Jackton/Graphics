﻿using System.Linq;
using System.Reflection;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    using CED = CoreEditorDrawer<SerializedFrameSettingsUI, SerializedFrameSettings>;

    [CustomEditor(typeof(HDRenderPipelineAsset))]
    public sealed partial class HDRenderPipelineInspector : HDBaseEditor<HDRenderPipelineAsset>
    {
        static readonly CED.IDrawer[] k_FrameSettings = new[]
            {
                SerializedFrameSettingsUI.SectionRenderingPasses,
                SerializedFrameSettingsUI.SectionRenderingSettings
            }.Concat(CED.Select(
                (s, d, o) => s.serializedLightLoopSettingsUI,
                (s, d, o) => d.lightLoopSettings,
                SerializedLightLoopSettingsUI.SectionLightLoopSettings))
            .Concat(new[]
            {
                SerializedFrameSettingsUI.SectionXRSettings
            })
            .ToArray();

        SerializedProperty m_RenderPipelineResources;

        // Global Frame Settings
        // Global Render settings
        SerializedProperty m_supportDBuffer;
        SerializedProperty m_supportMSAA;
        // Global Shadow settings
        SerializedProperty m_ShadowAtlasWidth;
        SerializedProperty m_ShadowAtlasHeight;
        // Global LightLoop settings
        SerializedProperty m_SpotCookieSize;
        SerializedProperty m_PointCookieSize;
        SerializedProperty m_ReflectionCubemapSize;
        // Commented out until we have proper realtime BC6H compression
        //SerializedProperty m_ReflectionCacheCompressed;
        SerializedProperty m_SkyReflectionSize;
        SerializedProperty m_SkyLightingOverrideLayerMask;

        // FrameSettings
        // LightLoop settings
        SerializedProperty m_enableTileAndCluster;
        SerializedProperty m_enableSplitLightEvaluation;
        SerializedProperty m_enableComputeLightEvaluation;
        SerializedProperty m_enableComputeLightVariants;
        SerializedProperty m_enableComputeMaterialVariants;
        SerializedProperty m_enableFptlForForwardOpaque;
        SerializedProperty m_enableBigTilePrepass;
        // Rendering Settings
        SerializedProperty m_RenderingUseForwardOnly;
        SerializedProperty m_RenderingUseDepthPrepass;
        SerializedProperty m_RenderingUseDepthPrepassAlphaTestOnly;
        SerializedProperty m_enableAsyncCompute;
        SerializedProperty m_enableShadowMask;

        // Subsurface Scattering Settings
        SerializedProperty m_SubsurfaceScatteringSettings;

        SerializedFrameSettings serializedFrameSettings = null;
        SerializedFrameSettingsUI SerializedFrameSettingsUI = new SerializedFrameSettingsUI();

        void InitializeProperties()
        {
            m_RenderPipelineResources = properties.Find("m_RenderPipelineResources");

            // Global FrameSettings
            // Global Render settings
            m_supportDBuffer = properties.Find(x => x.renderPipelineSettings.supportDBuffer);
            m_supportMSAA = properties.Find(x => x.renderPipelineSettings.supportMSAA);
            // Global Shadow settings
            m_ShadowAtlasWidth = properties.Find(x => x.renderPipelineSettings.shadowInitParams.shadowAtlasWidth);
            m_ShadowAtlasHeight = properties.Find(x => x.renderPipelineSettings.shadowInitParams.shadowAtlasHeight);
            // Global LightLoop settings

            m_SpotCookieSize = properties.Find(x => x.renderPipelineSettings.lightLoopSettings.spotCookieSize);
            m_PointCookieSize = properties.Find(x => x.renderPipelineSettings.lightLoopSettings.pointCookieSize);
            m_ReflectionCubemapSize = properties.Find(x => x.renderPipelineSettings.lightLoopSettings.reflectionCubemapSize);
            // Commented out until we have proper realtime BC6H compression
            //m_ReflectionCacheCompressed = properties.Find(x => x.globalFrameSettings.lightLoopSettings.reflectionCacheCompressed);
            m_SkyReflectionSize = properties.Find(x => x.renderPipelineSettings.lightLoopSettings.skyReflectionSize);
            m_SkyLightingOverrideLayerMask = properties.Find(x => x.renderPipelineSettings.lightLoopSettings.skyLightingOverrideLayerMask);

            // FrameSettings
            // LightLoop settings
            m_enableTileAndCluster = properties.Find(x => x.serializedFrameSettings.lightLoopSettings.enableTileAndCluster);
            m_enableComputeLightEvaluation = properties.Find(x => x.serializedFrameSettings.lightLoopSettings.enableComputeLightEvaluation);
            m_enableComputeLightVariants = properties.Find(x => x.serializedFrameSettings.lightLoopSettings.enableComputeLightVariants);
            m_enableComputeMaterialVariants = properties.Find(x => x.serializedFrameSettings.lightLoopSettings.enableComputeMaterialVariants);
            m_enableFptlForForwardOpaque = properties.Find(x => x.serializedFrameSettings.lightLoopSettings.enableFptlForForwardOpaque);
            m_enableBigTilePrepass = properties.Find(x => x.serializedFrameSettings.lightLoopSettings.enableBigTilePrepass);
            // Rendering Settings
            m_enableAsyncCompute = properties.Find(x => x.serializedFrameSettings.enableAsyncCompute);
            m_RenderingUseForwardOnly = properties.Find(x => x.serializedFrameSettings.enableForwardRenderingOnly);
            m_RenderingUseDepthPrepass = properties.Find(x => x.serializedFrameSettings.enableDepthPrepassWithDeferredRendering);
            m_RenderingUseDepthPrepassAlphaTestOnly = properties.Find(x => x.serializedFrameSettings.enableAlphaTestOnlyInDeferredPrepass);
            m_enableShadowMask = properties.Find(x => x.serializedFrameSettings.enableShadowMask);

            // Subsurface Scattering Settings
            m_SubsurfaceScatteringSettings = properties.Find(x => x.sssSettings);

            serializedFrameSettings = new SerializedFrameSettings(properties.Find(x => x.serializedFrameSettings));

            SerializedFrameSettingsUI.Reset(serializedFrameSettings, Repaint);
        }

        static void HackSetDirty(RenderPipelineAsset asset)
        {
            EditorUtility.SetDirty(asset);
            var method = typeof(RenderPipelineAsset).GetMethod("OnValidate", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
                method.Invoke(asset, new object[0]);
        }

        void GlobalLightLoopSettingsUI(HDRenderPipelineAsset hdAsset)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(s_Styles.textureSettings);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SpotCookieSize, s_Styles.spotCookieSize);
            EditorGUILayout.PropertyField(m_PointCookieSize, s_Styles.pointCookieSize);
            EditorGUILayout.PropertyField(m_ReflectionCubemapSize, s_Styles.reflectionCubemapSize);
            // Commented out until we have proper realtime BC6H compression
            //EditorGUILayout.PropertyField(m_ReflectionCacheCompressed, s_Styles.reflectionCacheCompressed);
            EditorGUILayout.PropertyField(m_SkyReflectionSize, s_Styles.skyReflectionSize);
            EditorGUILayout.PropertyField(m_SkyLightingOverrideLayerMask, s_Styles.skyLightingOverride);
            if (EditorGUI.EndChangeCheck())
            {
                HackSetDirty(hdAsset); // Repaint
            }
            EditorGUI.indentLevel--;
        }

        void GlobalRenderSettingsUI(HDRenderPipelineAsset hdAsset)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(s_Styles.renderingSettingsLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_supportDBuffer, s_Styles.supportDBuffer);
            EditorGUILayout.PropertyField(m_supportMSAA, s_Styles.supportMSAA);
            if (EditorGUI.EndChangeCheck())
            {
                HackSetDirty(hdAsset); // Repaint
            }
            EditorGUI.indentLevel--;
        }

        void GlobalShadowSettingsUI(HDRenderPipelineAsset hdAsset)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(s_Styles.shadowSettings);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ShadowAtlasWidth, s_Styles.shadowsAtlasWidth);
            EditorGUILayout.PropertyField(m_ShadowAtlasHeight, s_Styles.shadowsAtlasHeight);
            if (EditorGUI.EndChangeCheck())
            {
                HackSetDirty(hdAsset); // Repaint
            }
            EditorGUI.indentLevel--;
        }

        void SettingsUI(HDRenderPipelineAsset hdAsset)
        {
            EditorGUILayout.LabelField(s_Styles.renderPipelineSettings, EditorStyles.boldLabel);
            GlobalRenderSettingsUI(hdAsset);
            GlobalShadowSettingsUI(hdAsset);
            GlobalLightLoopSettingsUI(hdAsset);

            EditorGUILayout.Space();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InitializeProperties();
        }

        public override void OnInspectorGUI()
        {
            if (!m_Target || m_HDPipeline == null)
                return;

            CheckStyles();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_RenderPipelineResources, s_Styles.renderPipelineResources);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_SubsurfaceScatteringSettings, s_Styles.sssSettings);
            EditorGUILayout.Space();

            SettingsUI(m_Target);

            k_FrameSettings.Draw(SerializedFrameSettingsUI, serializedFrameSettings, this);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
