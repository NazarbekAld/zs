using Sandbox;

public sealed class PauseMenuManager : Component
{

	[Property]
	GameObject mainMenu;

	[Property]
	string closableUiTag = "ui-closable";

	protected override void OnUpdate()
	{
		if (IsProxy)
		{
			return;
		}


		var closeable = GameObject.Parent.Children.Find((ob) => ob.Tags.Has(closableUiTag));
		if (!(closeable is null)) {
			mainMenu.Enabled = false;
			return;
		}

		if (Input.EscapePressed) {
			mainMenu.Enabled = !mainMenu.Enabled;
		}
	}
}
