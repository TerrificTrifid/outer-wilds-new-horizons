﻿using NewHorizons.External.Configs;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.General
{
    public static class GravityBuilder
    {
        public static GravityVolume Make(GameObject planetGO, AstroObject ao, PlanetConfig config)
        {
            var exponent = config.Base.GravityFallOff.Equals("linear") ? 1f : 2f;
            var GM = config.Base.SurfaceGravity * Mathf.Pow(config.Base.SurfaceSize, exponent);

            // Gravity limit will be when the acceleration it would cause is less than 0.1 m/s^2
            var gravityRadius = GM / 0.1f;
            if (exponent == 2f) gravityRadius = Mathf.Sqrt(gravityRadius);

            // To let you actually orbit things the way you would expect we cap this at 4x the diameter if its not a star or black hole (this is what giants deep has)
            if (config.Star == null && config.Singularity == null) gravityRadius = Mathf.Min(gravityRadius, 4 * config.Base.SurfaceSize);
            else gravityRadius = Mathf.Min(gravityRadius, 15 * config.Base.SurfaceSize);
            if (config.Base.SphereOfInfluence != 0f) gravityRadius = config.Base.SphereOfInfluence;

            var gravityGO = new GameObject("GravityWell");
            gravityGO.transform.parent = planetGO.transform;
            gravityGO.transform.localPosition = Vector3.zero;
            gravityGO.layer = 17;
            gravityGO.SetActive(false);

            var SC = gravityGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = gravityRadius;

            var owCollider = gravityGO.AddComponent<OWCollider>();
            owCollider.SetLODActivationMask(DynamicOccupant.Player);

            var owTriggerVolume = gravityGO.AddComponent<OWTriggerVolume>();

            var gravityVolume = gravityGO.AddComponent<GravityVolume>();
            gravityVolume._cutoffAcceleration = 0.1f;

            GravityVolume.FalloffType falloff = GravityVolume.FalloffType.linear;
            if (config.Base.GravityFallOff.ToUpper().Equals("LINEAR")) falloff = GravityVolume.FalloffType.linear;
            else if (config.Base.GravityFallOff.ToUpper().Equals("INVERSESQUARED")) falloff = GravityVolume.FalloffType.inverseSquared;
            else Logger.LogError($"Couldn't set gravity type {config.Base.GravityFallOff}. Must be either \"linear\" or \"inverseSquared\". Defaulting to linear.");
            gravityVolume._falloffType = falloff;

            // Radius where your feet turn to the planet
            var alignmentRadius = config.Atmosphere?.Clouds?.OuterCloudRadius ?? 1.5f * config.Base.SurfaceSize;
            if (config.Base.SurfaceGravity == 0) alignmentRadius = 0;

            gravityVolume._alignmentRadius = alignmentRadius;
            gravityVolume._upperSurfaceRadius = config.Base.SurfaceSize;
            gravityVolume._lowerSurfaceRadius = 0;
            gravityVolume._layer = 3;
            gravityVolume._priority = 0;
            gravityVolume._alignmentPriority = 0;
            gravityVolume._surfaceAcceleration = config.Base.SurfaceGravity;
            gravityVolume._inheritable = false;
            gravityVolume._isPlanetGravityVolume = true;
            gravityVolume._cutoffRadius = 0f;

            gravityGO.SetActive(true);

            ao._gravityVolume = gravityVolume;

            return gravityVolume;
        }
    }
}
