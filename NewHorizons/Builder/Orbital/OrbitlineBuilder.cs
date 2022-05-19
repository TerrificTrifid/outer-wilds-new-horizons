﻿using NewHorizons.Components.Orbital;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Builder.Orbital
{
    public static class OrbitlineBuilder
    {
        private static Material _dottedLineMaterial;
        private static Material _lineMaterial;

        public static OrbitLine Make(GameObject planetGO, NHAstroObject astroObject, bool isMoon, PlanetConfig config)
        {
            if (_dottedLineMaterial == null) _dottedLineMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_SPA_OrbitLine_Dotted_mat");
            if (_lineMaterial == null) _lineMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_SPA_OrbitLine_mat");

            GameObject orbitGO = new GameObject("Orbit");
            orbitGO.transform.parent = planetGO.transform;
            orbitGO.transform.localPosition = Vector3.zero;

            var lineRenderer = orbitGO.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(config.Orbit.DottedOrbitLine ? _dottedLineMaterial : _lineMaterial);
            lineRenderer.textureMode = config.Orbit.DottedOrbitLine ? LineTextureMode.RepeatPerSegment : LineTextureMode.Stretch;

            var width = config.Orbit.DottedOrbitLine ? 100 : 50;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = false;

            var numVerts = config.Orbit.DottedOrbitLine ? 128 : 256;
            lineRenderer.positionCount = numVerts;

            var ecc = config.Orbit.Eccentricity;

            var parentGravity = astroObject.GetPrimaryBody()?.GetGravityVolume();

            OrbitLine orbitLine;
            if (config.Orbit.TrackingOrbitLine || (new Gravity(parentGravity).Power == 1 && ecc != 0))
            {
                orbitLine = orbitGO.AddComponent<TrackingOrbitLine>();
            }
            else
            {
                orbitLine = orbitGO.AddComponent<NHOrbitLine>();

                var a = astroObject.SemiMajorAxis;
                var e = astroObject.Eccentricity;
                var b = a * Mathf.Sqrt(1f - (e * e));
                var l = astroObject.LongitudeOfAscendingNode;
                var p = astroObject.ArgumentOfPeriapsis;
                var i = astroObject.Inclination;

                (orbitLine as NHOrbitLine).SemiMajorAxis = a * OrbitalParameters.Rotate(Vector3.left, l, i, p);
                (orbitLine as NHOrbitLine).SemiMinorAxis = b * OrbitalParameters.Rotate(Vector3.forward, l, i, p);
            }

            var color = Color.white;
            if (config.Orbit.Tint != null) color = config.Orbit.Tint.ToColor32();
            else if (config.Star != null) color = config.Star.Tint.ToColor32();
            else if (config.Atmosphere != null && config.Atmosphere.CloudTint != null) color = config.Atmosphere.CloudTint.ToColor32();
            else if (config.Singularity != null) color = new Color(1f, 0.5f, 1f);
            else if (config.Water != null) color = new Color(0.5f, 0.5f, 1f);
            else if (config.Lava != null) color = new Color(1f, 0.5f, 0.5f);
            else if (config.Atmosphere != null && config.Atmosphere.FogTint != null) color = config.Atmosphere.FogTint.ToColor32();

            var fade = isMoon;

            /*
            if (config.Base.IsSatellite)
            {
                if (config.Orbit.Tint != null) color = new Color(0.4082f, 0.516f, 0.4469f, 1f);
                fade = true;
                orbitLine._fadeEndDist = 5000;
                orbitLine._fadeStartDist = 3000;
            }
            */

            orbitLine._color = color;

            orbitLine._astroObject = astroObject;
            orbitLine._fade = fade;

            orbitLine._lineWidth = 0.2f;

            orbitLine._numVerts = (int)Mathf.Clamp(config.Orbit.SemiMajorAxis / 1000f, numVerts, 4096);

            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(orbitLine.InitializeLineRenderer);

            return orbitLine;
        }
    }
}
