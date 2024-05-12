#include "pch.h"
#include "mkl.h"
#include "mkldll.h"

enum class ErrorEnum { NO, INIT, CHECK, SOLVE, JACOBI, GET, DEL, RCI };


struct CubicSpline {

	int nX; // число узлов сплайна
	const double* grid; // границы равномерной сетки
	int nY; // размерность векторной функции
	int nS; // число узлов неравномерной сетки, на которой
	// вычисляются значения сплайна и его производных
	const double* X; // неравномерная сетка 
	const double* Y;
	int ErrCode;
	double* splineValues;
	double* scoeff;
	double* small_grid_values;
	int small_grid_size;

	CubicSpline(
		int nX, // число узлов сплайна
		const double* grid, // границы равномерной сетки
		const int nS, // число узлов неравномерной сетки, на которой
		// вычисляются значения сплайна и его производных
		const double* X, // неравномерная сетка
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

		int nX = ((CubicSpline*)splineData)->nX; // число узлов сплайна
		const double* grid = ((CubicSpline*)splineData)->grid; // границы равномерной сетки
		int nY = ((CubicSpline*)splineData)->nY; // размерность векторной функции
		int nS = ((CubicSpline*)splineData)->nS; // число узлов неравномерной сетки, на которой
		// вычисляются значения сплайна и его производных
		const double* X = ((CubicSpline*)splineData)->X; // неравномерная сетка 
		const double* Y = ((CubicSpline*)splineData)->Y;
		int& ErrCode = ((CubicSpline*)splineData)->ErrCode;
		double* splineValues = ((CubicSpline*)splineData)->splineValues;
		int small_grid_size = ((CubicSpline*)splineData)->small_grid_size;
		double* small_grid_values = ((CubicSpline*)splineData)->small_grid_values;

		MKL_INT s_order = DF_PP_CUBIC; // степень кубического сплайна
		MKL_INT s_type = DF_PP_NATURAL; // тип сплайна
		// тип граничных условий
		MKL_INT bc_type = DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER;
		// массив для коэффициентов сплайна
		double* scoeff = ((CubicSpline*)splineData)->scoeff;
		try
		{
			DFTaskPtr task;
			int status = -1;
			// Cоздание задачи (task)
			status = dfdNewTask1D(&task,
				nX, grid,
				DF_UNIFORM_PARTITION, // равномерная сетка узлов
				nY, app,
				DF_NO_HINT); // формат хранения значений векторной
			// функции по умолчанию (построчно)
			if (status != DF_STATUS_OK) throw 1;
			// Настройка параметров задачи
			double bc[2]{ 0, 0 }; //
			status = dfdEditPPSpline1D(task,
				s_order, s_type, bc_type, bc,
				DF_NO_IC, // тип условий во внутренних точках
				NULL, // массив значений для условий во внутренних точках
				scoeff,
				DF_MATRIX_STORAGE_ROWS); // формат упаковки коэффициентов сплайна
			// в одномерный массив (Row-major - построчно)
			if (status != DF_STATUS_OK) throw 2;
			// Создание сплайна
			status = dfdConstruct1D(task,
				DF_PP_SPLINE, // поддерживается только одно значение
				DF_METHOD_STD); // поддерживается только одно значение
			if (status != DF_STATUS_OK) throw 3;
			// Вычисление значений сплайна и его производных
			// вычисляются значения сплайна и производных
			int nDorder = 1; // число производных, которые вычисляются, плюс 1
			MKL_INT dorder[] = { 1 }; // вычисляются значения сплайна
			status = dfdInterpolate1D(task,
				DF_INTERP, // вычисляются значения сплайна и его производных
				DF_METHOD_PP, // поддерживается только одно значение
				nS, X,
				DF_NON_UNIFORM_PARTITION, // значения сплайна и его производных
				// вычисляются на неравномерной сетке
				nDorder, dorder,
				NULL, // нет дополнительной информации об узлах интерполяции
				ret,
				DF_MATRIX_STORAGE_ROWS, // формат упаковки результатов в одномерный массив
				NULL); // используется для ускорения вычислений на
			// неравномерной сетке; можно присвоить значение NULL	
			if (status != DF_STATUS_OK) throw 4;
			// Освобождение ресурсов
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
		int nX = ((CubicSpline*)splineData)->nX; // число узлов сплайна
		const double* grid = ((CubicSpline*)splineData)->grid; // границы равномерной сетки
		int nY = ((CubicSpline*)splineData)->nY; // размерность векторной функции
		int nS = ((CubicSpline*)splineData)->nS; // число узлов неравномерной сетки, на которой
		// вычисляются значения сплайна и его производных
		const double* X = ((CubicSpline*)splineData)->X; // неравномерная сетка 
		const double* Y = ((CubicSpline*)splineData)->Y;
		int& ErrCode = ((CubicSpline*)splineData)->ErrCode;
		double* splineValues = ((CubicSpline*)splineData)->splineValues;
		int small_grid_size = ((CubicSpline*)splineData)->small_grid_size;
		double* small_grid_values = ((CubicSpline*)splineData)->small_grid_values;

		MKL_INT s_order = DF_PP_CUBIC; // степень кубического сплайна
		MKL_INT s_type = DF_PP_NATURAL; // тип сплайна
		// тип граничных условий
		MKL_INT bc_type = DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER;
		// массив для коэффициентов сплайна
		double* scoeff = ((CubicSpline*)splineData)->scoeff;
		try
		{
			DFTaskPtr task;
			int status = -1;
			// Cоздание задачи (task)
			status = dfdNewTask1D(&task,
				nX, grid,
				DF_UNIFORM_PARTITION, // равномерная сетка узлов
				nY, app,
				DF_NO_HINT); // формат хранения значений векторной
			// функции по умолчанию (построчно)
			if (status != DF_STATUS_OK) throw 1;
			// Настройка параметров задачи
			double bc[2]{ 0, 0 }; //
			status = dfdEditPPSpline1D(task,
				s_order, s_type, bc_type, bc,
				DF_NO_IC, // тип условий во внутренних точках
				NULL, // массив значений для условий во внутренних точках
				scoeff,
				DF_MATRIX_STORAGE_ROWS); // формат упаковки коэффициентов сплайна
			// в одномерный массив (Row-major - построчно)
			if (status != DF_STATUS_OK) throw 2;
			// Создание сплайна
			status = dfdConstruct1D(task,
				DF_PP_SPLINE, // поддерживается только одно значение
				DF_METHOD_STD); // поддерживается только одно значение
			if (status != DF_STATUS_OK) throw 3;
			// Вычисление значений сплайна и его производных
			// вычисляются значения сплайна и производных
			int nDorder = 1; // число производных, которые вычисляются, плюс 1
			MKL_INT dorder[] = { 1 }; // вычисляются значения сплайна
			status = dfdInterpolate1D(task,
				DF_INTERP, // вычисляются значения сплайна и его производных
				DF_METHOD_PP, // поддерживается только одно значение
				small_grid_size, grid,
				DF_UNIFORM_PARTITION, // значения сплайна и его производных
				// вычисляются на неравномерной сетке
				nDorder, dorder,
				NULL, // нет дополнительной информации об узлах интерполяции
				small_grid_values,
				DF_MATRIX_STORAGE_ROWS, // формат упаковки результатов в одномерный массив
				NULL); // используется для ускорения вычислений на
			// неравномерной сетке; можно присвоить значение NULL	
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
	MKL_INT n, // число независимых переменных
	MKL_INT m, // число компонент векторной функции
	double* x, // начальное приближение и решение
	CubicSpline& Spline, // указатель на функцию, вычисляющую векторную
	// функцию в заданной точке
	const double* eps, // массив с 6 элементами, определяющих критерии
	// остановки итерационного процесса
	double jac_eps, // точность вычисления элементов матрицы Якоби
	MKL_INT niter1, // максимальное число итераций
	MKL_INT niter2, // максимальное число итераций при выборе пробного шага
	double rs, // начальный размер доверительной области
	MKL_INT& ndoneIter, // число выполненных итераций
	double& resInitial, // начальное значение невязки
	double& resFinal, // финальное значение невязки
	MKL_INT& stopCriteria,// выполненный критерий остановки
	MKL_INT* checkInfo, // информация об ошибках при проверке данных
	ErrorEnum& error) // информация об ошибках
{
	_TRNSP_HANDLE_t handle = NULL; // переменная для дескриптора задачи
	double* fvec = NULL; // массив значений векторной функции
	double* fjac = NULL; // массив с элементами матрицы Якоби
	error = ErrorEnum(ErrorEnum::NO);
	try
	{
		fvec = new double[m]; // массив значений векторной функции
		fjac = new double[n * m]; // массив с элементами матрицы Якоби
		// Инициализация задачи
		MKL_INT ret = dtrnlsp_init(&handle, &n, &m, x, eps, &niter1, &niter2, &rs);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::INIT));
		// Проверка корректности входных данных
		ret = dtrnlsp_check(&handle, &n, &m, fjac, fvec, eps, checkInfo);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::CHECK));
		MKL_INT RCI_Request = 0; // надо инициализировать 0 !!!
		// Итерационный процесс
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
		// Завершение итерационного процесса
		ret = dtrnlsp_get(&handle, &ndoneIter, &stopCriteria,
			&resInitial, &resFinal);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::GET));
		// Освобождение ресурсов
		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS) throw (ErrorEnum(ErrorEnum::DEL));
	}
	catch (ErrorEnum _error) { error = _error; }
	// Освобождение памяти
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
	MKL_INT n = M; // число независимых переменных
	MKL_INT m = N; // число компонент векторной функции
	double* x = new double[M]; // массив с начальным приближением и решением

	for (int i = 0; i < M; ++i) x[i] = 0;

	MKL_INT niter1 = maxiter; // максимальное число итераций
	MKL_INT niter2 = maxiter; // максимальное число итераций при выборе
	// пробного шага
	MKL_INT& ndone_iter = numiter; // число выполненных итераций
	double rs = 10; // начальное значение для
	// доверительного интервала
	const double eps[6] = // массив критериев остановки
	{ 1e-12 , // размер доверительной области
	stopr , // норма целевой функции
	1e-12 , // норма строк матрицы Якоби
	1e-12 , // точность пробного шага
	1e-12 , // разность нормы целевой функции
		// и погрешности аппроксимации функции
	1e-12 }; // точность вычисления пробного шага
	double jac_eps = 1.0E-8; // точность вычисления элементов матрицы Якоби
	// Возвращаемые значения
	double res_initial = 0; // начальное значение невязки
	double res_final = 0; // финальное значение невязки
	MKL_INT stop_criteria; // причина остановки итераций
	MKL_INT check_data_info[4]; // результат проверки корректности данных
	ErrorEnum error = ErrorEnum(ErrorEnum::NO); // информация об ошибке

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