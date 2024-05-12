#pragma once
extern "C" __declspec(dllexport) void MakeApproximation(
	const double* X,
	const double* Y,
	int M,
	int N,
	double* spline,
	int maxiter,
	double stopr,
	int num_small_grid,
	double* values,
	MKL_INT& numiter,
	int& errorcode,
	double& mindis
);
