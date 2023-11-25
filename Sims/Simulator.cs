//============================================================================
// Simulator.cs : Defines the base class for creating simulations.
//============================================================================
using System;

public class Simulator
{
    protected int n;           // number of first order odes
    protected double[] x;      // array of states
    protected double[] xi;     // array of intermediate states
    protected double[][] f;    // 2d array that holds values of rhs

    protected double g;                  // gravitational field strength

    private Action<double[], double, double[]> rhsFunc;

    //--------------------------------------------------------------------
    // Constructor
    //--------------------------------------------------------------------
    public Simulator(int nn)
    {
        g = 9.81; 

        n = nn;
        x = new double[n];
        xi = new double[n];
        f = new double[4][];
        f[0] = new double[n];
        f[1] = new double[n];
        f[2] = new double[n];
        f[3] = new double[n];

        rhsFunc = nothing;
    }

    //--------------------------------------------------------------------
    // StepEuler: Executes one numerical integration step using Euler's 
    //            method.
    //--------------------------------------------------------------------
    public void StepEuler(double time, double dTime)
    {
        int i;

        rhsFunc(x,time,f[0]);
        for(i=0;i<n;++i)
        {
            x[i] += f[0][i] * dTime;
        }
    }

    //--------------------------------------------------------------------
    // StepRK2: Executes one numerical integration step using the RK2 
    //            method.
    //--------------------------------------------------------------------
    public void StepRK2(double time, double dTime)
    {
        int i;

        rhsFunc(x,time,f[0]);
        for(i=0;i<n;++i)
        {
            xi[i] = x[i] + f[0][i] * dTime;
        }

        rhsFunc(xi,time+dTime,f[1]);
        for(i=0;i<n;++i)
        {
            x[i] += 0.5*(f[0][i] + f[1][i])*dTime;
        }       
    }

    //--------------------------------------------------------------------
    // Step: Executes one numerical integration step using the RK4 
    //            method.
    //--------------------------------------------------------------------
    public void Step(double time, double dTime)
    {
        int i;

        double dtByTwo = 0.5*dTime;

        rhsFunc(x,time,f[0]);
        for(i=0;i<n;++i)
        {
            xi[i] = x[i] + f[0][i] * dtByTwo;
        }

        rhsFunc(xi,time+dtByTwo,f[1]);
        for(i=0;i<n;++i)
        {
            xi[i] = x[i] + f[1][i] * dtByTwo;
        }

        rhsFunc(xi,time+dtByTwo,f[2]);
        for(i=0;i<n;++i)
        {
            xi[i] = x[i] + f[2][i] * dTime;
        }

        rhsFunc(xi,time+dTime,f[3]);
        for(i=0;i<n;++i)
        {
            x[i] += (f[0][i] + 2.0*f[1][i] + 2.0*f[2][i] + f[3][i]) * 
                dTime/6.0;
        }
    }

    //--------------------------------------------------------------------
    // SetRHSFunc: Receives function from derived class to calculate 
    //             rhs of ODE.
    //--------------------------------------------------------------------
    protected void SetRHSFunc(Action<double[],double,double[]> rhs)
    {
        rhsFunc = rhs;
    }

    private void nothing(double[] st,double t,double[] ff)
    {

    }

}