public interface IInitializeable
{
	void init();
	signals.Signal initializationComplete { get; }
    bool initialized { get; }
}
