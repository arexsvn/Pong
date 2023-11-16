public interface IResettable
{
	void reset();
	signals.Signal resetComplete { get; }
}
