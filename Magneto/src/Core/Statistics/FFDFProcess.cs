using Magneto.Contract.Interface;

namespace Core.Statistics;

public class FfdfProcess(IAntennaController antennaController) : DataProcessBase(antennaController);