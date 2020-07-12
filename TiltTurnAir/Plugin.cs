using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using IllusionPlugin;
using UnityEngine;
using UnityEngineInternal;

namespace TiltTurnAir
{
	public class Plugin : IPlugin
	{
        const float headTiltPow = 3f;
        int currentLevelId = 0;
		const float fallSpeedOffset = 1.5f;
		const float fallSpeedScale = 0.015f;
		const float generalSpeedCoeff = 0.3f;
		const float maxForceAdded = 0.5f;
		const float tiltOffset = -0.0003f;

		public string Name
		{
			get { return "TiltTurnAir"; }
		}
		public string Version
		{
			get { return "0.15"; }
		}

		public void OnApplicationQuit()
		{
		}

		public void OnApplicationStart()
		{
		}

		public void OnFixedUpdate()
		{
		}

		public void OnLevelWasInitialized(int level)
		{
			currentLevelId = level;
			if (currentLevelId != 1)
				return;
		}

		public void OnLevelWasLoaded(int level)
		{
		}

		public void OnUpdate()
		{
			// Only activate if the level is the main island (i.e. not the menu).
			if (currentLevelId != 1)
				return;

			// Only turn while in the air and hoverboard is deployed.
			if (PlayerBody.localPlayer.movement.sliding && !PlayerBody.localPlayer.movement.grounded)
			{
				// Initialize direction and motion vectors.
				Vector3 worldUp = Vector3.up;
				Vector3 moveOrig = PlayerBody.localPlayer.movement.currentMovement;
				Vector3 lookOrig = PlayerBody.localPlayer.body.headParent.forward;
				Vector3 lookOrigUp = PlayerBody.localPlayer.body.headParent.up;
				Vector3 lookOrigXZ = Vector3.ProjectOnPlane(lookOrig, worldUp);

				// Isolate the Z component of the player's head tilt.
				Vector3 headTiltProjectedXZ = Vector3.ProjectOnPlane(lookOrigUp, Vector3.up);
				Vector3 headTiltProjectedZ = Vector3.ProjectOnPlane(headTiltProjectedXZ, lookOrigXZ);

				// Control of the board increases with air velocity, weighted towards downward motion over forward/lateral.
				float fallSpeed = Mathf.Max(0, (Vector3.Project(moveOrig, worldUp).y * -1) * fallSpeedScale) + fallSpeedOffset
					+ (Mathf.Log(Vector3.ProjectOnPlane(moveOrig, worldUp).magnitude + 1) * generalSpeedCoeff);

				// Limit the maximum force to be applied.
				if (headTiltProjectedZ.magnitude * fallSpeed > maxForceAdded)
                {
					headTiltProjectedZ.Normalize();
					headTiltProjectedZ *= maxForceAdded;
                }

				// Apply the finalized force vector to the player's movement.
				PlayerBody.localPlayer.movement.currentMovement += (headTiltProjectedZ * Mathf.Max(0, fallSpeed * (Mathf.Pow(headTiltProjectedZ.magnitude, headTiltPow) + tiltOffset)));
			}
		}
	}
}
