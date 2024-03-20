using Sandbox;

public sealed class MenuManager : Component
{

	[Property]
	PrefabScene mainMenuPref;

	GameObject mainMenu;

	[Property]
	string closableUiTag = "ui-closable";

	protected override void OnUpdate()
	{
		var closeable = GameObject.Parent.Children.Find((ob) => ob.Tags.Has(closableUiTag));

		if (closeable is not null)
		{
			if ( mainMenu is not null)
			{
				mainMenu.Destroy();
				mainMenu = null;

			}
			return;
		}
		if (mainMenu is null) {
			mainMenu = mainMenuPref.Clone();
			return;
		}

		
	}
}
