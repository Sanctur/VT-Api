﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VT_Api.Patches.VtEvent.MapPaches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdDetonateWarhead))]
    class StartingWarHead
    {
        [HarmonyPrefix]
        private static bool NuckStartPatch(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute() && !__instance.CanInteract)
                    return false;
                GameObject gameObject = GameObject.Find("OutsitePanelScript");
                if (!__instance.ChckDis(gameObject.transform.position) || !AlphaWarheadOutsitePanel.nukeside.enabled || !gameObject.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
                    return false;
                bool flag = true;
                VtController.Get.Events.Map.InvokeWarHeadStartEvent(__instance.GetPlayer(), ref flag);
                return flag;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Vt-Event: DetonateWarhead failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
