﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace PetRenamer
{
	internal class PRPlayer : ModPlayer
	{
		internal int petTypeVanity;
		internal string petNameVanity;
		internal int petTypeLight;
		internal string petNameLight;

		internal Item renamePetUIItem;

		private int prevItemType;

		private bool OpenedChatWithMouseItem => !Main.chatRelease && PetRenamer.IsPetItem(Main.mouseItem);

		private bool MouseItemChangedToPetItem => prevItemType != Main.mouseItem.type && PetRenamer.IsPetItem(Main.mouseItem);

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (PetRenamer.RenamePetUIHotkey.JustPressed)
			{
				PRUISystem.ToggleRenamePetUI();
			}
		}

		public override void Initialize()
		{
			petTypeVanity = 0;
			petNameVanity = string.Empty;
			petTypeLight = 0;
			petNameLight = string.Empty;

			renamePetUIItem = new Item();

			prevItemType = 0;
		}

		public override void SaveData(TagCompound tag)
		{
			if (!renamePetUIItem.IsAir)
			{
				tag.Add("renamePetUIItem", renamePetUIItem);
			}
		}

		public override void LoadData(TagCompound tag)
		{
			renamePetUIItem = tag.Get<Item>("renamePetUIItem");
			if (renamePetUIItem == null)
			{
				renamePetUIItem = new Item();
			}
		}

		public override void PostUpdateEquips()
		{
			UpdatePets();

			//Only do the autocomplete in chat on the client
			if (Main.netMode != NetmodeID.Server && Main.myPlayer == Player.whoAmI)
			{
				Autocomplete();
			}
		}

		private void UpdatePets()
		{
			SetTypeAndNameOfCurrentEquippedPetInSlot(PetRenamer.VANITY_PET, ref petTypeVanity, ref petNameVanity);
			SetTypeAndNameOfCurrentEquippedPetInSlot(PetRenamer.LIGHT_PET, ref petTypeLight, ref petNameLight);
		}

		private void SetTypeAndNameOfCurrentEquippedPetInSlot(int slot, ref int type, ref string name)
		{
			Item item = Player.miscEquips[slot];
			if (!Player.hideMisc[slot] && item.TryGetGlobalItem(out PRItem petItem))
			{
				type = item.shoot;
				name = petItem.petName;
			}
		}

		private void Autocomplete()
		{
			if (!Config.Instance.EnableChatAutofill)
			{
				return;
			}

			if (Main.drawingPlayerChat &&
					(OpenedChatWithMouseItem || MouseItemChangedToPetItem))
			{
				if (!Main.chatText.StartsWith(PRCommand.CommandStart) && Main.chatText.Length == 0)
				{
					ChatManager.AddChatText(FontAssets.MouseText.Value, PRCommand.CommandStart, Vector2.One);
				}
			}

			prevItemType = Main.mouseItem?.type ?? 0;
		}
	}
}
