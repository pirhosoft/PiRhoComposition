namespace PiRhoSoft.Composition
{
	public class DisableGraphTrigger : GraphTrigger
	{
		void OnDisable()
		{
			Run();
		}
	}
}
