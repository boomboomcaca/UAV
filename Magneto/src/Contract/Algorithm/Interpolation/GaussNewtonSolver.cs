using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Magneto.Contract.Algorithm.Interpolation;

internal class GaussNewtonSolver(
    double minimumDeltaValue,
    double minimumDeltaParameters,
    int maximumIterations,
    Vector<double> guess)
{
    private Vector<double> _dataX;
    private Vector<double> _dataY;

    /// <summary>
    ///     Get value of the objective function.
    /// </summary>
    /// <param name="model">Model function.</param>
    /// <param name="pointCount">Number of data points.</param>
    /// <param name="parameters">Model function parameters.</param>
    /// <param name="value">Objective function value.</param>
    private void GetObjectiveValue(PowerModel model, int pointCount, Vector<double> parameters, out double value)
    {
        value = 0.0;
        for (var j = 0; j < pointCount; j++)
        {
            model.GetValue(_dataX[j], parameters, out var y);
            value += Math.Pow(y - _dataY[j], 2.0);
        }

        value *= 0.5;
    }

    /// <summary>
    ///     Get Jacobian matrix of the objective function.
    /// </summary>
    /// <param name="model">Model function.</param>
    /// <param name="pointCount">Number of data points.</param>
    /// <param name="parameters">Model function parameters.</param>
    /// <param name="jacobian">Jacobian matrix of the objective function.</param>
    private void GetObjectiveJacobian(PowerModel model, int pointCount, Vector<double> parameters,
        ref Matrix<double> jacobian)
    {
        var parameterCount = parameters.Count;
        // fill rows of the Jacobian matrix
        // j-th row of a Jacobian is the gradient of model function in j-th measurement
        for (var j = 0; j < pointCount; j++)
        {
            Vector<double> gradient = new DenseVector(parameterCount);
            model.GetGradient(
                _dataX[j],
                parameters,
                ref gradient);
            jacobian.SetRow(j, gradient);
        }
    }

    /// <summary>
    ///     Estimates the specified model.
    /// </summary>
    /// <param name="model">Model function.</param>
    /// <param name="pointCount">Number of data points.</param>
    /// <param name="dataX">X-coordinates of the data points.</param>
    /// <param name="dataY">Y-coordinates of the data points.</param>
    /// <param name="iterations">Estimated model function parameters.</param>
    public void Estimate(PowerModel model, int pointCount, Vector<double> dataX, Vector<double> dataY,
        ref List<Vector<double>> iterations)
    {
        _dataX = dataX;
        _dataY = dataY;
        var n = guess.Count;
        var parametersCurrent = guess;
        Vector<double> parametersNew = new DenseVector(n);
        GetObjectiveValue(model, pointCount, parametersCurrent, out var valueCurrent);
        while (true)
        {
            Matrix<double> jacobian = new DenseMatrix(pointCount, n);
            Vector<double> residual = new DenseVector(pointCount);
            GetObjectiveJacobian(model, pointCount, parametersCurrent, ref jacobian);
            model.GetResidualVector(pointCount, dataX, dataY, parametersCurrent, ref residual);
            var step = jacobian.Transpose().Multiply(jacobian).Cholesky()
                .Solve(jacobian.Transpose().Multiply(residual));
            parametersCurrent.Subtract(step, parametersNew);
            GetObjectiveValue(model, pointCount, parametersNew, out var valueNew);
            iterations.Add(parametersNew);
            if (ShouldTerminate(valueCurrent, valueNew, iterations.Count, parametersCurrent, parametersNew)) break;
            parametersNew.CopyTo(parametersCurrent);
            valueCurrent = valueNew;
        }
    }

    /// <summary>
    ///     Check whether the solver should terminate computation in current iteration.
    /// </summary>
    /// <param name="valueCurrent">Current value of the objective function.</param>
    /// <param name="valueNew">New value of the objective function.</param>
    /// <param name="iterationCount">Number of computed iterations.</param>
    /// <param name="parametersCurrent">Current estimated model parameters.</param>
    /// <param name="parametersNew">New estimated model parameters.</param>
    /// <returns>The solver should terminate computation in current iteration.</returns>
    private bool ShouldTerminate(double valueCurrent, double valueNew, int iterationCount,
        Vector<double> parametersCurrent, Vector<double> parametersNew)
    {
        return
            Math.Abs(valueNew - valueCurrent) <= minimumDeltaValue ||
            parametersNew.Subtract(parametersCurrent).Norm(2.0) <= minimumDeltaParameters ||
            iterationCount >= maximumIterations;
    }
}