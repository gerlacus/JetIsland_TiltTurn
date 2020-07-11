using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using IllusionPlugin;
using UnityEngine;

namespace TiltTurnGround
{
	public class Plugin : IPlugin
	{
		int currentLevelId = 0;
		float turnScale = 0.05f;
		float turnOffset = -0.0001f;
		float playerSpeedMultiplier;
		float turnSpeedTotal;

		public string Name
		{
			get { return "TiltTurnGround"; }
		}
		public string Version
		{
			get { return "0.1"; }
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

			// Only turn while on the ground and hoverboard is deployed.
			if (PlayerBody.localPlayer.movement.sliding && PlayerBody.localPlayer.movement.grounded)
			{
				// Initialize player's direction and motion vectors.
				Vector3 moveOrig = PlayerBody.localPlayer.movement.currentMovement;
				Vector3 lookOrig = PlayerBody.localPlayer.body.headParent.forward;
				Vector3 lookOrigUp = PlayerBody.localPlayer.body.headParent.up;
				Vector3 groundNormalOrig = PlayerBody.localPlayer.movement.groundNormal;

				// Set player's "up" based on world space if in standard mode and on local space if in Extreme Spin mode.
				Vector3 playerUp = PlayerBody.localPlayer.modifiers.modifiersActive.ExtremeRotationMode ? groundNormalOrig : Vector3.up;

				// Isolate the Z component of the player's head tilt.
				Vector3 headTiltProjectedXZ = Vector3.ProjectOnPlane(lookOrigUp, playerUp);
				Vector3 headTiltProjectedZ = Vector3.ProjectOnPlane(headTiltProjectedXZ, lookOrig);
				Vector3 headTiltProjectedZGround = Vector3.ProjectOnPlane(headTiltProjectedZ, groundNormalOrig);

				// Apply the Z component to a mild forward vector for more predictable directional control.
				Vector3 lookProjectedGround = Vector3.ProjectOnPlane(lookOrig, groundNormalOrig);
				Vector3 headTiltProjectedZGroundForward = Vector3.ProjectOnPlane(headTiltProjectedZ, groundNormalOrig) + (lookProjectedGround * 0.5f);

				// Scale the turn speed based on the player's current velocity and apply a deadzone to the player's head tilt.
				playerSpeedMultiplier = 1f + PlayerBody.localPlayer.movement.currentMovement.magnitude;
				turnSpeedTotal = Math.Max(0f, (turnScale * Time.deltaTime * playerSpeedMultiplier * (float)Math.Pow(headTiltProjectedZGround.magnitude, 3f)) + turnOffset);

				// Apply the finalized rotation operation to the player's movement.
				PlayerBody.localPlayer.movement.currentMovement = Vector3.RotateTowards(
					moveOrig,
					headTiltProjectedZGroundForward,
					turnSpeedTotal,
					0f);
			}
		}
	}
}
