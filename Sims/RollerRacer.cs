//============================================================================
// RollerRacer.cs : Defines derived class for simulating a Roller Racer.
//       Equations of motion are derived in class notes.
//============================================================================
using System;

public class RollerRacer : Simulator
{
    // physical parameters, names are the same as that in the notes
    double m;   // mass of vehicle
    double Ig;  // moment of inertia (vertical axis) about center of mass
    double b;   // distance of com ahead of rear axle
    double c;   // distance of rear contact patch from symmetry axis
    double d;   // caster length
    double h;   // longitudinal distance between center of mass and steer axis

    double rW;  // radius of rear wheel, used for calculating rotation rate
    double rWs; // radius of steered wheel, used for calculating rotation rate

    double kPDelta;  // proportional gain for steer filter
    double kDDelta;  // derivative gain for steer filter
    double deltaDes; // desired steer angle
    
    double kPSlip;   // proportional gain for slip error control
    double muS;      // static frict coeff, lower bound

    double FbrakeMax; // maximum braking force
    double brakeVelTH; // velocity threshold for braking
    double brakeSignal;// brakeSignal

    LinAlgEq sys;    // system of linear algebraic equations

    bool simBegun;   // indicates whether simulation has begun

    public RollerRacer() : base(11)
    {
        g = 9.81;
        muS = 0.9;
        SetInertia(25.0 /*mass*/, 0.3 /*radius of gyration*/);
        SetGeometry(1.3 /*wheel base*/, 0.6 /* cg dist from axle*/,
            0.3 /*caster dist*/, 1.0 /*wheel sep*/, 0.5*0.75 /*Rwheel radius*/,
            0.15 /*steered wheel radius*/);
        kPDelta = 10.0;
        kDDelta = 4.0;
        kPSlip = 2.0;

        x[0] = 0.0;   // x coordinate of center of mass
        x[1] = 0.0;   // xDot, time derivative of x
        x[2] = 0.0;   // z coordinate of center of mass
        x[3] = 0.0;   // zDot, time derivative of z
        x[4] = 0.0;   // psi, heading angle
        x[5] = 0.0;   // psiDot, time derivative of heading, yaw rate
        x[6] = 0.0;   // rotation angle of left rear wheel
        x[7] = 0.0;   // rotation angle of right rear wheel
        x[8] = 0.0;   // rotation angle of front steered wheel
        x[9] = 0.0;   // delta, steer angle
        x[10] = 0.0;  // deltaDot, steer rate

        SetRHSFunc(RHSFuncRRacer);

        sys = new LinAlgEq(5);

        simBegun = false;
    }

    private void RHSFuncRRacer(double[] xx, double t, double[] ff)
    {
        // give names to some state variable so code is easier to read & write
        double xDot = xx[1];
        double zDot = xx[3];
        double psi  = xx[4];
        double psiDot = xx[5];
        double delta = xx[9];
        double deltaDot = xx[10];

        // calculate some trig functions here, so you only have to do it once
        double cosPsi = Math.Cos(psi);
        double sinPsi = Math.Sin(psi);
        double cosDelta = Math.Cos(delta);
        double sinDelta = Math.Sin(delta);
        double cosPsiPlusDelta = Math.Cos(psi + delta);
        double sinPsiPlusDelta = Math.Sin(psi + delta);

        // #### You will do some hefty calculations here
        double deltaDDot = -kDDelta*deltaDot -kPDelta*(delta - deltaDes);
        double slipRateRear = xDot*sinPsi + zDot*cosPsi + b*psiDot;
        double slipRateFront = xDot*sinPsiPlusDelta + zDot*cosPsiPlusDelta -
            h*psiDot*cosDelta + (psiDot + deltaDot)*d;
        
        // braking
        double axlVel = xDot*cosPsi - zDot*sinPsi;
        double brakeForce = 0.0;
        if(Math.Abs(axlVel) > brakeVelTH){  // coulomb braking
            brakeForce = -Math.Sign(axlVel)*FbrakeMax;
        }
        else{   // viscous braking
            brakeForce = -FbrakeMax*axlVel/brakeVelTH;
        }
        brakeForce *= brakeSignal;

        // equation (1) from notes
        sys.A[0][0] = m;
        sys.A[0][1] = 0.0;
        sys.A[0][2] = 0.0;
        sys.A[0][3] = -sinPsi;
        sys.A[0][4] = -sinPsiPlusDelta;
        sys.b[0] = brakeForce*cosPsi;

        // equation (2) from notes
        sys.A[1][0] = 0.0;
        sys.A[1][1] = m;
        sys.A[1][2] = 0.0;
        sys.A[1][3] = -cosPsi;
        sys.A[1][4] = -cosPsiPlusDelta;
        sys.b[1] = -brakeForce*sinPsi;

        // equation (3) from notes
        sys.A[2][0] = 0.0;
        sys.A[2][1] = 0.0;
        sys.A[2][2] = Ig;
        sys.A[2][3] = -b;
        sys.A[2][4] = h*cosDelta - d;
        sys.b[2] = 0.0;

        // equation (7) from notes
        sys.A[3][0] = sinPsi;
        sys.A[3][1] = cosPsi;
        sys.A[3][2] = b;
        sys.A[3][3] = 0.0;
        sys.A[3][4] = 0.0;
        sys.b[3] = -xDot*psiDot*cosPsi + zDot*psiDot*sinPsi - 
            kPSlip*slipRateRear;

        // equation (10) from notes
        double dum = psiDot + deltaDot;
        sys.A[4][0] = sinPsiPlusDelta;
        sys.A[4][1] = cosPsiPlusDelta;
        sys.A[4][2] = d-h*cosDelta;
        sys.A[4][3] = 0.0;
        sys.A[4][4] = 0.0;
        sys.b[4] = -d*deltaDDot - xDot*dum*cosPsiPlusDelta +
            zDot*dum*sinPsiPlusDelta - h*psiDot*deltaDot*sinDelta -
            kPSlip*slipRateFront;

        sys.SolveGauss();

        ff[0] = xDot;
        ff[1] = sys.sol[0];
        ff[2] = zDot;
        ff[3] = sys.sol[1];
        ff[4] = psiDot;
        ff[5] = sys.sol[2];
        ff[6] = -(xDot*cosPsi - zDot*sinPsi - c*psiDot)/rW;
        ff[7] = -(xDot*cosPsi - zDot*sinPsi + c*psiDot)/rW;;
        ff[8] = -(xDot*cosPsiPlusDelta - zDot*sinPsiPlusDelta + 
            h*psiDot*sinDelta)/rWs;
        ff[9] = deltaDot;
        ff[10] = deltaDDot;

        simBegun = true;
    }

    //------------------------------------------------------------------------
    // SetInitialSpeed: Sets the initial speed of the vehicle. Must be set
    //          before simulation has begun.
    //------------------------------------------------------------------------
    public void SetInitalSpeed(double val)
    {
        if(simBegun) return;

        x[1] = val;

    }

    //------------------------------------------------------------------------
    // SetInertia: sets the two inertia properties of the vehicle. 
    //     mm: total mass in kilograms
    //     rgyr: radius of gyration in meters
    //------------------------------------------------------------------------
    public void SetInertia(double mm, double rgyr)
    {
        if(mm <= 0.1)   // check lower bound for mass
            return;     // return and not update parameters.

        if(rgyr < 0.03) // check lower bound for radius of gyration
            return;     // return and not update parameters.

        m = mm;
        Ig = m*rgyr*rgyr;

        FbrakeMax = 0.3*m*g;
        brakeVelTH = 0.1;
        brakeSignal = 0.0;
    }

    //------------------------------------------------------------------------
    // SetGeometry: Sets the geometry of the vehicle.
    //    wsb: distance between rear axle and steer axis
    //    dcg: distance from wheel axle to center of mass
    //    dcst: length of the caster
    //    wid: distance between rear wheels
    //    wRad: radius of rear wheel
    //    wRadS: radius of steered wheel
    //------------------------------------------------------------------------
    public void SetGeometry(double wsb, double dcg, double dcst, double wid, 
        double wRad, double wRadS)
    {
        // check lower bounds
        if(wsb < 0.01) return;
        if(dcg <= 0.0) return;
        if(dcst < 0.0) return;
        if(wid < 0.05) return;
        if(wRad < 0.05) return;
        if(wRadS < 0.05) return;

        if(wsb-dcst < dcg) return; //cg must be btw rear axle and steer contact

        b = dcg;
        c = 0.5*wid;
        d = dcst;
        h = wsb-dcg;

        rW = wRad;
        rWs = wRadS;
    }

    //------------------------------------------------------------------------
    // Getters/Setters
    //------------------------------------------------------------------------

    public double BrakeSignal
    {
        set{
            double bSig = value;
            if(bSig < 0.0) bSig = 0.0;
            if(bSig > 1.0) bSig = 1.0;
            brakeSignal = bSig;
        }
    }

    public double SteerAngleSignal
    {
        set{
            deltaDes = value;
        }
    }

    public double SteerAngle
    {
        get{
            return x[9];
        }
    }

    public double xG
    {
        get{
            return x[0];
        }
    }

    public double zG
    {
        get{
            return x[2];
        }
    }

    public double Heading
    {
        get{
            return x[4];
        }
    }

    public double WheelAngleL
    {
        get{
            return x[6];
        }
    }

    public double WheelAngleR
    {
        get{
            return x[7];
        }
    }

    public double WheelAngleF
    {
        get{
            return x[8];
        }
    }

    public double Speed
    {
        get{

            return(Math.Sqrt(x[1]*x[1] + x[3]*x[3]));
        }
    }

    public double KineticEnergy
    {
        get{
            double ke1 = 0.5*m*(x[1]*x[1] + x[3]*x[3]);
            double ke2 = 0.5*Ig*x[5]*x[5];
            return(ke1+ke2);
        }
    }

    public double SlipRateFront
    {
        get{
            double slip = x[1]*Math.Sin(x[4]+x[9]) +
                x[3]*Math.Cos(x[4]+x[9]) - h*x[5]*Math.Cos(x[9]) +
                (x[5] + x[10])*d;

            return(slip);
        }
    }

    public double SlipRateRear
    {
        get{
            double slip = x[1]*Math.Sin(x[4]) + x[3]*Math.Cos(x[4]) +
                b*x[5];
            return(slip);
        }
    }

    public double FontFrictionFactor
    {
        get{
            // ######## You have to write this part ################

            return(-1.21212121);
        }
    }
}