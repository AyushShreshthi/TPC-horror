using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TPC
{
    public static class Statics
    {

        #region hash
        public static string horizontal = "horizontal";
        public static string vertical = "vertical";
        public static string special = "special";
        public static string specialType = "specialType";
        public static string onLocomotion = "onLocomotion";
        public static string Horizontal = "Horizontal";
        public static string Vertical = "Vertical";
        public static string jumpType = "jumpType";
        public static string Jump = "Jump";
        public static string onAir = "onAir";
        public static string mirrorJump = "mirrorJump";
        public static string incline = "incline";
        public static string Fire3 = "Fire3";
        public static string inSpecial = "inSpecial";
        public static string walkVault = "vault_over_walk_1";
        public static string runVault = "vault_over_run";
        public static string walk_up = "walk_up";
        public static string run_up = "run_up";
        public static string onSprint = "onSprint";
        public static string climb_up = "climb_up_high";

        #endregion

        #region Variables
        public static float vaultCheckDistance =1f;
        public static float vaultSpeedWalking = 2;
        public static float vaultSpeedRunning = 4;
        public static float vaultSpeedIdle = 1;
        public static float vaultCheckDistance_Run =2f;
        public static float walkupSpeed = 1f;
        public static float climbMaxHeight = 2f;//1
        public static float walkupHeight =1f;
        public static float climbSpeed = 0.5f;
        public static float climbUpStartPosOffset = 0.5f;

        #endregion

        #region functions
        public static int GetAnimSpecialType(AnimSpecials i)
        {
            int r = 0;
            switch (i)
            {
                case AnimSpecials.runToStop:
                    r = 11;
                    break;
                case AnimSpecials.run:
                    r = 10;
                    break;
                case AnimSpecials.jump_idle:
                    r = 21;
                    break;
                case AnimSpecials.run_jump:
                    r = 22;
                    break;
                default:
                    break;
            }
            return r;
        }
        #endregion
    }

    public enum AnimSpecials
    {
        run,runToStop,jump_idle,run_jump
    }
}
