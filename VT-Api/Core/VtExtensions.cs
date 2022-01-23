﻿using Synapse.Api;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VT_Api.Core.Enum;
using VT_Api.Core.Roles;

namespace VT_Api.Extension
{
    static internal class VtExtensions
    {
        internal static void Debug(this Synapse.Api.Logger logger, object message)
            => logger.Send($"VtApi-Debug: {message}", ConsoleColor.DarkYellow);

        public static bool IsDefined(this SerializedPlayerInventory item)
            => item.Ammo != null || (item.Items != null && item.Items.Any());

        public static bool IsDefined(this SynapseItem item)
            => item != null && item != SynapseItem.None && item.ItemType != ItemType.None;

        public static void ChangeRoomLightColor(this Room room, Color color, bool activeColor = true)
        {
            room.LightController.WarheadLightColor = color;
            room.LightController.WarheadLightOverride = activeColor;
        }

        public static bool Is939(this RoleType roleType)
            => roleType == RoleType.Scp93953 || roleType == RoleType.Scp93989;

        public static bool Is939(this RoleID roleID)
            => roleID == RoleID.Scp93953 || roleID == RoleID.Scp93989;

        public static T GetOrAddComponent<T>(this Component component) where T : Component
            => component.gameObject.GetOrAddComponent<T>();

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T Component;
            if (!gameObject.TryGetComponent(out Component))
                Component = gameObject.AddComponent<T>();
            return Component;
        }

        public static void Extract(this SerializedPlayerRole playerRole, Player player, out Vector3 postion, out Vector2 rotation, out List<SynapseItem> items, out Dictionary<AmmoType, ushort> ammos)
        {
            postion = playerRole.SpawnPoint.Parse().Position;

            if (playerRole.Rotation != null)
                rotation = playerRole.Rotation;
            else rotation = Vector2.zero;

            if (playerRole.Inventory != null)
                playerRole.Inventory.Extract(player, out items, out ammos);
            else
            {
                items = new List<SynapseItem>();
                ammos = new Dictionary<AmmoType, ushort>();
            }
        }


        public static void Extract(this SerializedPlayerInventory playerInventory, Player player, out List<SynapseItem> items, out Dictionary<AmmoType, ushort> ammos)
        {
            items = new List<SynapseItem>();

            if (playerInventory.Items != null) foreach (var item in playerInventory.Items)
            {
                if (item.Extract(player, out var synapseItem))  
                    items.Add(synapseItem);
            }

            if (playerInventory.Ammo != null)
                playerInventory.Ammo.Extract(out ammos);
            else ammos = null;
        }

        public static void Extract(this SerializedAmmo serializedAmmo, out Dictionary<AmmoType, ushort> ammos)
        {
            ammos = new Dictionary<AmmoType, ushort>();
            ammos.Add(AmmoType.Ammo556x45, serializedAmmo.Ammo5);
            ammos.Add(AmmoType.Ammo762x39, serializedAmmo.Ammo7);
            ammos.Add(AmmoType.Ammo9x19, serializedAmmo.Ammo9);
            ammos.Add(AmmoType.Ammo12gauge, serializedAmmo.Ammo12);
            ammos.Add(AmmoType.Ammo44cal, serializedAmmo.Ammo44);
        }

        public static bool Extract(this SerializedPlayerItem serializedItem, Player player, out SynapseItem item)
        {
            item = serializedItem.Parse();
            if (serializedItem.UsePreferences && item.ItemCategory == ItemCategory.Firearm)
                item.WeaponAttachments = player.GetPreference(ItemManager.Get.GetBaseType(serializedItem.ID));

            return UnityEngine.Random.Range(1f, 100f) <= serializedItem.Chance;
        }

        public static ScpRecontainmentType GetScpRecontainmentType(this DamageType damage, Player player = null)
        {
            switch (damage)
            {
                case DamageType.Warhead:
                    return ScpRecontainmentType.Nuke;
                case DamageType.Decontamination:
                    return ScpRecontainmentType.Decontamination;
                case DamageType.Tesla:
                    return ScpRecontainmentType.Tesla;
                default:
                    if (player == null)
                        return ScpRecontainmentType.Unknown;
                    if (player.TeamID <= (int)TeamID.CDP && player.TeamID >= (int)TeamID.NTF)
                        return (ScpRecontainmentType)player.TeamID;
                    return ScpRecontainmentType.Unspecified;
            }
        }

        public static bool TryAddOrDrop(this PlayerInventory playerInv, SynapseItem item)
        {
            if (playerInv.Items.Count < 8)
            {
                playerInv.AddItem(item);
                return true;
            }
            else
            {
                playerInv.Drop(item);
                return false;
            }
        }

        public static bool TryPickupOrDrop(this SynapseItem item, Player player)
        {
            if (player.Inventory.Items.Count < 8)
            {
                item.PickUp(player);
                return true;
            }
            else
            {
                item.Drop(player.Position);
                return false;
            }
        }

        public static bool TryPickup(this Player.PlayerAmmoBox ammos, SynapseItem item)
        {
            if (item.ItemCategory != ItemCategory.Ammo)
            {
                return false;
            }
            else
            {
                ammos[(AmmoType)item.ItemType] = (ushort)item.Durabillity;
                return true;
            }
        }

        public static void SetDisplayInfoRole(this Player player, string roleName)
        {
            player.RemoveDisplayInfo(PlayerInfoArea.Role);

            if (player.Team == Team.MTF)
            {
                player.RemoveDisplayInfo(PlayerInfoArea.UnitName);
                player.DisplayInfo = $"{roleName} ({player.UnitName})";
            }
            else
            {
                player.DisplayInfo = roleName;
            }
        }

    }
}