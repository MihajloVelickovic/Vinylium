[SetUpFixture]
public class GlobalTestSetup{
	[OneTimeSetUp]
	public void PinEnvironment(){
		Environment.SetEnvironmentVariable("CANCELLATION_TIME", "24");
	}
}
