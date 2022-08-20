namespace NeosModLoader
{
	internal class LoadedNeosMod
	{
		internal LoadedNeosMod(NeosMod neosMod, AssemblyFile modAssembly)
		{
			NeosMod = neosMod;
			ModAssembly = modAssembly;
		}

		internal NeosMod NeosMod { get; private set; }
		internal AssemblyFile ModAssembly { get; private set; }
		internal ModConfiguration? ModConfiguration { get; set; }
		internal bool AllowSavingConfiguration = true;
		internal bool FinishedLoading { get => NeosMod.FinishedLoading; set => NeosMod.FinishedLoading = value; }
		internal string Name { get => NeosMod.Name; }
	}
}
