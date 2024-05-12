#include "pch.h"
#include "mkl.h"
#include "mkldll.h"

enum class ErrorEnum { NO, INIT, CHECK, SOLVE, JACOBI, GET, DEL, RCI };


struct CubicSpline {

	int nX; // ����� ����� �������
	const double* grid; // ������� ����������� �����
	int nY; // ����������� ��������� �������
	int nS; // ����� ����� ������������� �����, �� �������
	// ����������� �������� ������� � ��� �����������
	const double* X; // ������������� ����� 
	const double* Y;
	int ErrCode;
	double* splineValues;
	double* scoeff;
	double* small_grid_values;
	int small_grid_size;

	CubicSpline(
		int nX, // ����� ����� �������
		const double* grid, // ������� ����������� �����
		const int nS, // ����� ����� ������������� �����, �� �������
		// ����������� �������� ������� � ��� �����������
		const double* X, // ������������� �����
		const double* Y,
		double* splineValues,
		int small_grid_size,
		double* small_grid_values
	)
	{
		this->small_grid_values = small_grid_values;
		//scoeff = new double[nY * (nX - 1) * DF_PP_CUBIC];
		this->nX = nX;
		this->grid = grid;
		this->nS = nS;
		this->X = X;
		this->Y = Y;
		this->splineValues = splineValues;
		this->small_grid_size = small_grid_size;
		nY = 1;
		ErrCode = 0;
		scoeff = new double[nY * (nX - 1) * DF_PP_CUBIC];
	}
	static void FValues(MKL_INT* m, MKL_INT* n, double* app, double* ret, void* splineData)
	{

		int nX = ((CubicSpline*)splineData)->nX; // ����� ����� �������
		const double* grid = ((CubicSpline*)splineData)->grid; // ������� ����������� �����
		int nY = ((CubicSpline*)splineData)->nY; // ����������� ��������� �������
		int nS = ((CubicSpline*)splineData)->nS; // ����� ����� ������������� �����, �� �������
		// ����������� �������� ������� � ��� �����������
		const double* X = ((CubicSpline*)splineData)->X; // ������������� ����� 
		const double* Y = ((CubicSpline*)splineData)->Y;
		int& ErrCode = ((CubicSpline*)splineData)->ErrCode;
		double* splineValues = ((CubicSpline*)splineData)->splineValues;
		int small_grid_size = ((CubicSpline*)splineData)->small_grid_size;
		double* small_grid_values = ((CubicSpline*)splineData)->small_grid_values;

		MKL_INT s_order = DF_PP_CUBIC; // ������� ����������� �������
		MKL_INT s_type = DF_PP_NATURAL; // ��� �������
		// ��� ��������� �������
		MKL_INT bc_type = DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER;
		// ������ ��� ������������� �������
		double* scoeff = ((CubicSpline*)splineData)->scoeff;
		try
		{
			DFTaskPtr task;
			int status = -1;
			// C������� ������ (task)
			status = dfdNewTask1D(&task,
				nX, grid,
				DF_UNIFORM_PARTITION, // ����������� ����� �����
				nY, app,
				DF_NO_HINT); // ������ �������� �������� ���������
			// ������� �� ��������� (���������)
			if (status != DF_STATUS_OK) throw 1;
			// ��������� ���������� ������
			double bc[2]{ 0, 0 }; //
			status = dfdEditPPSpline1D(task,
				s_order, s_type, bc_type, bc,
				DF_NO_IC, // ��� ������� �� ���������� ������
				NULL, // ������ �������� ��� ������� �� ���������� ������
				scoeff,
				DF_MATRIX_STORAGE_ROWS); // ������ �������� ������������� �������
			// � ���������� ������ (Row-major - ���������)
			if (status != DF_STATUS_OK) throw 2;
			// �������� �������
			status = dfdConstruct1D(task,
				DF_PP_SPLINE, // �������������� ������ ���� ��������
				DF_METHOD_STD); // �������������� ������ ���� ��������
			if (status != DF_STATUS_OK) throw 3;
			// ���������� �������� ������� � ��� �����������
			// ����������� �������� ������� � �����������
			int nDorder = 1; // ����� �����������, ������� �����������, ���� 1
			MKL_INT dorder[] = { 1 }; // ����������� �������� �������
			status = dfdInterpolate1D(task,
				DF_INTERP, // ����������� �������� ������� � ��� �����������
				DF_METHOD_PP, // �������������� ������ ���� ��������
				nS, X,
				DF_NON_UNIFORM_PARTITION, // �������� ������� � ��� �����������
				// ����������� �� ������������� �����
				nDorder, dorder,
				NULL, // ��� �������������� ���������� �� ����� ������������
				ret,
				DF_MATRIX_STORAGE_ROWS, // ������ �������� ����������� � ���������� ������
				NULL); // ������������ ��� ��������� ���������� ��
			// ������������� �����; ����� ��������� �������� NULL	
			if (status != DF_STATUS_OK) throw 4;
			// ������������ ��������
			for (int i = 0; i < nS; ++i) {
				splineValues[i] = ret[i];
				ret[i] = Y[i] - ret[i];
			}

			status = dfDeleteTask(&task);
			if (status != DF_STATUS_OK) throw 5;
		}
		catch (int ret)
		{
			ErrCode = ret;
		}

		ErrCode = 0;
	}
	static void Smallgrid(double* app, void* splineData)
	{
		int nX = ((CubicSpline*)splineData)->nX; // ����� ����� �������
		const double* grid = ((CubicSpline*)splineData)->grid; // ������� ����������� �����
		int nY = ((CubicSpline*)splineData)->nY; // ����������� ��������� �������
		int nS = ((CubicSpline*)splineData)->nS; // ����� ����� ������������� �����, �� �������
		// ����������� �������� ������� � ��� �����������
		const double* X = ((CubicSpline*)splineData)->X; // ������������� ����� 
		const double* Y = ((CubicSpline*)splineData)->Y;
		int& ErrCode = ((CubicSpline*)splineData)->ErrCode;
		double* splineValues = ((CubicSpline*)splineData)->splineValues;
		int small_grid_size = ((CubicSpline*)splineData)->small_grid_size;
		double* small_grid_values = ((CubicSpline*)splineData)->small_grid_values;

		MKL_INT s_order = DF_PP_CUBIC; // ������� ����������� �������
		MKL_INT s_type = DF_PP_NATURAL; // ��� �������
		// ��� ��������� �������
		MKL_INT bc_type = DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER;
		// ������ ��� ������������� �������
		double* scoeff = ((CubicSpline*)splineData)->scoeff;
		try
		{
			DFTaskPtr task;
			int status = -1;
			// C������� ������ (task)
			status = dfdNewTask1D(&task,
				nX, grid,
				DF_UNIFORM_PARTITION, // ����������� ����� �����
				nY, app,
				DF_NO_HINT); // ������ �������� �������� ���������
			// ������� �� ��������� (���������)
			if (status != DF_STATUS_OK) throw 1;
			// ��������� ���������� ������
			double bc[2]{ 0, 0 }; //
			status = dfdEditPPSpline1D(task,
				s_order, s_type, bc_type, bc,
				DF_NO_IC, // ��� ������� �� ���������� ������
				NULL, // ������ �������� ��� ������� �� ���������� ������
				scoeff,
				DF_MATRIX_STORAGE_ROWS); // ������ �������� ������������� �������
			// � ���������� ������ (Row-major - ���������)
			if (status != DF_STATUS_OK) throw 2;
			// �������� �������
			status = dfdConstruct1D(task,
				DF_PP_SPLINE, // �������������� ������ ���� ��������
				DF_METHOD_STD); // �������������� ������ ���� ��������
			if (status != DF_STATUS_OK) throw 3;
			// ���������� �������� ������� � ��� �����������
			// ����������� �������� ������� � �����������
			int nDorder = 1; // ����� �����������, ������� �����������, ���� 1
			MKL_INT dorder[] = { 1 }; // ����������� �������� �������
			status = dfdInterpolate1D(task,
				DF_INTERP, // ����������� �������� ������� � ��� �����������
				DF_METHOD_PP, // �������������� ������ ���� ��������
				small_grid_size, grid,
				DF_UNIFORM_PARTITION, // �������� ������� � ��� �����������
				// ����������� �� ������������� �����
				nDorder, dorder,
				NULL, // ��� �������������� ���������� �� ����� ������������
				small_grid_values,
				DF_MATRIX_STORAGE_ROWS, // ������ �������� ����������� � ���������� ������
				NULL); // ������������ ��� ��������� ���������� ��
			// ������������� �����; ����� ��������� �������� NULL	
			if (status != DF_STATUS_OK) throw 4;

			status = dfDeleteTask(&task);
			if (status != DF_STATUS_OK) throw 5;
		}
		catch (int ret)
		{
			ErrCode = ret;
		}

		//ErrCode = 0;
	}
};

bool TrustRegion(
	MKL_INT n, // ����� ����������� ����������
	MKL_INT m, // ����� ��������� ��������� �������
	double* x, // ��������� ����������� � �������
	CubicSpline& Spline, // ��������� �� �������, ����������� ���������
	// ������� � �������� �����
	const double* eps, // ������ � 6 ����������, ������������ ��������
	// ��������� ������������� ��������
	double jac_eps, // �������� ���������� ��������� ������� �����
	MKL_INT niter1, // ������������ ����� ��������
	MKL_INT niter2, // ������������ ����� �������� ��� ������ �������� ����
	double rs, // ��������� ������ ������������� �������
	MKL_INT& ndoneIter, // ����� ����������� ��������
	double& resInitial, // ��������� �������� �������
	double& resFinal, // ��������� �������� �������
	MKL_INT& stopCriteria,// ����������� �������� ���������
	MKL_INT* checkInfo, // ���������� �� ������� ��� �������� ������
	ErrorEnum& error) // ���������� �� �������
{
	_TRNSP_HANDLE_t handle = NULL; // ���������� ��� ����������� ������
	double* fvec = NULL; // ������ �������� ��������� �������
	double* fjac = NULL; // ������ � ���������� ������� �����
	error = ErrorEnum(ErrorEnum::NO);
	try
	{
		fvec = new double[m]; // ������ �������� ��������� �������
		fjac = new double[n * m]; // ������ � ���������� ������� �����
		// ������������� ������
		MKL_INT ret = dtrnlsp_init(&handle, &n, &m, x, eps, &niter1, &niter2, &rs);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::INIT));
		// �������� ������������ ������� ������
		ret = dtrnlsp_check(&handle, &n, &m, fjac, fvec, eps, checkInfo);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::CHECK));
		MKL_INT RCI_Request = 0; // ���� ���������������� 0 !!!
		// ������������ �������
		while (true)
		{
			ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);
			if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::SOLVE));
			if (RCI_Request == 0) continue;
			else if (RCI_Request == 1) Spline.FValues(&m, &n, x, fvec, &Spline);
			else if (RCI_Request == 2)
			{
				ret = djacobix(CubicSpline::FValues, &n, &m, fjac, x, &jac_eps, &Spline);
				if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::JACOBI));
			}
			else if (RCI_Request >= -6 && RCI_Request <= -1) break;
			else throw (ErrorEnum(ErrorEnum::RCI));
		}
		// ���������� ������������� ��������
		ret = dtrnlsp_get(&handle, &ndoneIter, &stopCriteria,
			&resInitial, &resFinal);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::GET));
		// ������������ ��������
		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::DEL));
	}
	catch (ErrorEnum _error) { error = _error; }
	// ������������ ������
	//if (fvec != NULL) delete[] fvec;
	//if (fjac != NULL) delete[] fjac;
}

void MakeApproximation(
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
	int& stop,
	double& mindis
) {
	MKL_INT n = M; // ����� ����������� ����������
	MKL_INT m = N; // ����� ��������� ��������� �������
	double* x = new double[M]; // ������ � ��������� ������������ � ��������

	for (int i = 0; i < M; ++i) x[i] = 0;

	MKL_INT niter1 = maxiter; // ������������ ����� ��������
	MKL_INT niter2 = maxiter; // ������������ ����� �������� ��� ������
	// �������� ����
	MKL_INT& ndone_iter = numiter; // ����� ����������� ��������
	double rs = 10; // ��������� �������� ���
	// �������������� ���������
	const double eps[6] = // ������ ��������� ���������
	{ 1e-12 , // ������ ������������� �������
	stopr , // ����� ������� �������
	1e-12 , // ����� ����� ������� �����
	1e-12 , // �������� �������� ����
	1e-12 , // �������� ����� ������� �������
		// � ����������� ������������� �������
	1e-12 }; // �������� ���������� �������� ����
	double jac_eps = 1.0E-8; // �������� ���������� ��������� ������� �����
	// ������������ ��������
	double res_initial = 0; // ��������� �������� �������
	double res_final = 0; // ��������� �������� �������
	MKL_INT stop_criteria; // ������� ��������� ��������
	MKL_INT check_data_info[4]; // ��������� �������� ������������ ������
	ErrorEnum error = ErrorEnum(ErrorEnum::NO); // ���������� �� ������

	double* grid = new double[2] {X[0], X[N - 1]};

	CubicSpline cs = CubicSpline(M, grid, N, X, Y, spline, num_small_grid, values);

	TrustRegion(n, m, x, cs, eps, jac_eps, niter1, niter2, rs,
		ndone_iter, res_initial, res_final, stop_criteria,
		check_data_info, error);

	cs.Smallgrid(x, &cs);

	mindis = res_final;
	stop = (int)stop_criteria;
	//delete[] cs.scoeff;
	//delete[] x;
}