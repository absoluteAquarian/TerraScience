using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.UI;

namespace TerraScience.Content.TileEntities{
	public abstract class MachineEntity : ModTileEntity{
		private readonly List<Item> slots = new List<Item>();

		public Item GetItem(int slot) => slots[slot];

		internal void ValidateSlots(int intendedLength){
			//Possible if the multitile was just placed
			if(slots.Count != intendedLength){
				slots.Clear();
				for(int i = 0; i < intendedLength; i++){
					Item item = new Item();
					slots.Add(item);
				}
			}
		}

		/// <summary>
		/// The multiplier for the reaction progress.
		/// </summary>
		public float ReactionSpeed = 1f;

		/// <summary>
		/// The progress for the current reaction.
		/// Range: [0, 100]
		/// </summary>
		public float ReactionProgress = 0f;

		public bool ReactionInProgress = false;

		public MachineUI ParentState;

		public string MachineName;

		public abstract int MachineTile{ get; }

		public abstract int SlotsCount{ get; }

		public sealed override bool ValidTile(int i, int j){
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.active() && tile.type == MachineTile && tile.frameX == 0 && tile.frameY == 0;
		}

		public virtual void PreUpdateReaction(){ }

		/// <summary>
		/// Update <seealso cref="ReactionProgress"/> here.  Return true to indicate that a reaction is supposed to happen, false otherwise.
		/// </summary>
		public abstract bool UpdateReaction();

		/// <summary>
		/// Called after <seealso cref="ReactionComplete()"/>, but not necessarily when a reaction is completed.
		/// </summary>
		public virtual void PostUpdateReaction(){ }

		/// <summary>
		/// Actions that should happen when a reaction is complete goes here.
		/// </summary>
		public abstract void ReactionComplete();

		/// <summary>
		/// Always called.  General update task after the reaction has been handled.
		/// </summary>
		public virtual void PostReaction(){ }

		/// <summary>
		/// If this machine must have its UI open in order to update.
		/// </summary>
		public virtual bool RequiresUI => false;

		public sealed override TagCompound Save()
			=> new TagCompound(){
				["machineInfo"] = new TagCompound(){
					[nameof(ReactionSpeed)] = ReactionSpeed,
					[nameof(ReactionProgress)] = ReactionProgress,
					[nameof(ReactionInProgress)] = ReactionInProgress
				},
				["slots"] = new TagCompound(){
					//Lots of unnecessary data is saved, but that's fine due to the small amount of extra bytes used
					// TODO: refactor ItemIO.Save/ItemIO.Load to get rid of this extra info
					["items"] = slots.Count == 0 ? null : slots.Select(i => ItemIO.Save(i)).ToList()
				},
				["extra"] = ExtraSave()
			};

		public virtual TagCompound ExtraSave() => null;

		public sealed override void Load(TagCompound tag){
			TagCompound info = tag.GetCompound("machineInfo");
			ReactionSpeed = info.GetFloat(nameof(ReactionSpeed));
			ReactionProgress = info.GetFloat(nameof(ReactionProgress));
			ReactionInProgress = info.GetBool(nameof(ReactionInProgress));

			TagCompound tagSlots = tag.GetCompound("slots");
			List<TagCompound> items = tagSlots.GetList<TagCompound>("items") as List<TagCompound> ?? new List<TagCompound>();
			
			foreach(var c in items)
				slots.Add(ItemIO.Load(c));

			TagCompound extra = tag.GetCompound("extra");
			if(extra != null)
				ExtraLoad(extra);
		}

		public virtual void ExtraLoad(TagCompound tag){ }

		public sealed override void Update(){
			if(RequiresUI && !(ParentState?.Active ?? false))
				return;

			ValidateSlots(SlotsCount);

			PreUpdateReaction();

			if(ReactionInProgress && UpdateReaction()){
				if(ReactionProgress >= 100){
					ReactionComplete();

					//In case a derived class forgets to reset the reaction progress
					if(ReactionProgress >= 100)
						ReactionProgress -= 100;
				}

				PostUpdateReaction();
			}

			PostReaction();
		}

		public void SaveSlots(){
			slots.Clear();

			for(int i = 0; i < ParentState.SlotsLength; i++){
				int type = ParentState.GetSlot(i).StoredItem.type;
				int stack = ParentState.GetSlot(i).StoredItem.stack;

				Item item = new Item();
				item.SetDefaults(type);
				item.stack = stack;

				slots.Add(item);
			}
		}

		public void LoadSlots(){
			ParentState.LoadToSlots(slots);
		}

		public override void OnKill(){
			//Force the UI to close if it's open
			if(ParentState?.Active ?? false)
				TerraScience.Instance.machineLoader.HideUI(MachineName);
		}
	}
}
