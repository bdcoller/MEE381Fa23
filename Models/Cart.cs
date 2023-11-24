using Godot;
using System;

public partial class Cart : Node3D
{
	Node3D wheelFrameL;
	Node3D wheelFrameR;
	Node3D steerFrame;
	Node3D wheelFrameF;

	float wheelRad;        // rear wheel radius
	float wheelRadS;       // radius of steered wheel
	float wheelSep;        // separation of rear wheels, center to center
	float baseLen;         // base length rear axle to steer axis
	float casterLen;       // caster length for front wheel
	float tireWd;         // rear tire width
	float wheelWd;        // rear wheel width
	float tireWdS;        // steered tire width
	float wheelWdS;       // steered wheel width
	float barWidth;        // width of bar
	
	float cgDist;          // dist of cg ahead of wheel axle

	Vector3 cgLoc;         // location of cg pojected on y=0 plane
	Vector3 rotVec;        // rotation about vertical axis
	Vector3 rotWhL;        // rotation vector for left wheel
	Vector3 rotWhR;        //                     right wheel
	Vector3 rotWhF;        //                     front wheel
	Vector3 rotSteer;      // rotation vector for steer axis

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		// default parameters
		wheelRad = 0.5f * 0.75f;
		wheelRadS = 0.15f;
		wheelSep = 1.0f;
		baseLen = 1.3f;
		casterLen = 1.0f*0.30f;
		tireWd = 0.08f;
		wheelWd = 0.5f * tireWd;
		tireWdS = 0.05f;
		wheelWdS = 0.7f * tireWdS;
		barWidth = 0.08f;
		cgDist = 0.6f;

		wheelFrameL = GetNode<Node3D>("CartFrame/WheelFrameL");
		wheelFrameR = GetNode<Node3D>("CartFrame/WheelFrameR");
		steerFrame = GetNode<Node3D>("CartFrame/SteerFrame");
		wheelFrameF = GetNode<Node3D>("CartFrame/SteerFrame/WheelFrameF");
		BuildModel();
		
		cgLoc  = new Vector3();
		rotVec = new Vector3();
		rotWhL = new Vector3();
		rotWhR = new Vector3();
		rotWhF = new Vector3();
		rotSteer = new Vector3();
	}

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		
	}

	//------------------------------------------------------------------------
	// BuildModel:
	//------------------------------------------------------------------------
	public void BuildModel()
	{
		// axle bar
		MeshInstance3D axleBar = GetNode<MeshInstance3D>("CartFrame/AxleBar");
		BoxMesh axleBarMesh = (BoxMesh)axleBar.Mesh;
		float axleBarLen = wheelSep - 1.5f*wheelWd;
		axleBarMesh.Size = new Vector3(barWidth, barWidth, axleBarLen);

		// longitudinal bar
		MeshInstance3D longBar = GetNode<MeshInstance3D>("CartFrame/LongBar");
		BoxMesh longBarMesh = (BoxMesh)longBar.Mesh;
		longBarMesh.Size = new Vector3(baseLen+barWidth, barWidth, barWidth);
		float shft = 0.5f*(baseLen+barWidth) + 0.5f*barWidth;
		longBar.Position = new Vector3(shft, 0.0f, 0.0f);

		// left wheel
		MeshInstance3D tireL = 
			GetNode<MeshInstance3D>("CartFrame/WheelFrameL/Tire");
		TorusMesh tireMeshL = (TorusMesh)tireL.Mesh;
		tireMeshL.OuterRadius = wheelRad;
		tireMeshL.InnerRadius = wheelRad - tireWd;
		MeshInstance3D wheelL = 
			GetNode<MeshInstance3D>("CartFrame/WheelFrameL/Wheel");
		CylinderMesh wheelMeshL = (CylinderMesh)wheelL.Mesh;
		wheelMeshL.TopRadius = wheelRad - 0.5f*tireWd;
		wheelMeshL.BottomRadius = wheelRad - 0.5f*tireWd;
		wheelMeshL.Height = wheelWd;
		wheelFrameL.Position = new Vector3(0.0f, 0.0f, -0.5f*wheelSep);

		// right wheel
		MeshInstance3D tireR = 
			GetNode<MeshInstance3D>("CartFrame/WheelFrameR/Tire");
		TorusMesh tireMeshR = (TorusMesh)tireR.Mesh;
		tireMeshR.OuterRadius = wheelRad;
		tireMeshR.InnerRadius = wheelRad - tireWd;
		MeshInstance3D wheelR = 
			GetNode<MeshInstance3D>("CartFrame/WheelFrameR/Wheel");
		CylinderMesh wheelMeshR = (CylinderMesh)wheelR.Mesh;
		wheelMeshR.TopRadius = wheelRad - 0.5f*tireWd;
		wheelMeshR.BottomRadius = wheelRad - 0.5f*tireWd;
		wheelMeshR.Height = wheelWd;
		wheelFrameR.Position = new Vector3(0.0f, 0.0f, 0.5f*wheelSep);

		// steered wheel
		MeshInstance3D tireS = 
			GetNode<MeshInstance3D>("CartFrame/SteerFrame/WheelFrameF/Tire");
		TorusMesh tireMeshS = (TorusMesh)tireS.Mesh;
		tireMeshS.OuterRadius = wheelRadS;
		tireMeshS.InnerRadius = wheelRadS - tireWdS;
		MeshInstance3D wheelS = 
			GetNode<MeshInstance3D>("CartFrame/SteerFrame/WheelFrameF/Wheel");
		CylinderMesh wheelMeshS = (CylinderMesh)wheelS.Mesh;
		wheelMeshS.TopRadius = wheelRadS - 0.5f*tireWdS;
		wheelMeshS.BottomRadius = wheelRadS - 0.5f*tireWdS;
		wheelMeshS.Height = wheelWd;
		float yCtr = -wheelRad + wheelRadS;
		wheelFrameF.Position = new Vector3(-casterLen, yCtr, 0.0f);

		// steer axle
		MeshInstance3D steerAxle = 
			GetNode<MeshInstance3D>("CartFrame/SteerFrame/SteerAxle");
		CylinderMesh stAxleMesh = (CylinderMesh)steerAxle.Mesh;
		stAxleMesh.TopRadius = 0.45f*barWidth;
		stAxleMesh.BottomRadius = 0.45f*barWidth;
		float dum = 2.0f*wheelRad - wheelRadS + 0.5f*barWidth;
		stAxleMesh.Height = dum;
		steerAxle.Position = new Vector3(0.0f, wheelRad-0.5f*dum, 0.0f);

		// handlebar
		MeshInstance3D handlebar = 
			GetNode<MeshInstance3D>("CartFrame/SteerFrame/HandleBar");
		CylinderMesh hBarMesh = (CylinderMesh)handlebar.Mesh;
		hBarMesh.TopRadius = 0.45f*barWidth;
		hBarMesh.BottomRadius = 0.45f*barWidth;
		stAxleMesh.Height = 0.6f*wheelSep;
		handlebar.Position = new Vector3(0.0f, wheelRad, 0.0f);

		steerFrame.Position = new Vector3(baseLen, 0.0f, 0.0f);

		// Shift cart frame
		Node3D cartFrame = GetNode<Node3D>("CartFrame");
		cartFrame.Position = new Vector3(-cgDist, wheelRad, 0.0f);
	}

	//------------------------------------------------------------------------
	// SetLoc: Sets (xG,zG) center of mass coordinates, heading angle (rad),
	//         wheel angles (rad), and steer angle
	//------------------------------------------------------------------------
	public void SetLoc(float xG, float zG, float hdg, float thL, float thR,
		float thF, float stAngle)
	{
		cgLoc.X = xG;
		cgLoc.Z = zG;
		rotVec.Y = hdg;
		rotWhL.Z = thL;
		rotWhR.Z = thR;
		rotWhF.Z = thF;
		rotSteer.Y = stAngle;

		Position = cgLoc;
		Rotation = rotVec;
		wheelFrameL.Rotation = rotWhL;
		wheelFrameR.Rotation = rotWhR;
		wheelFrameF.Rotation = rotWhF;
		steerFrame.Rotation = rotSteer;
	}

	//------------------------------------------------------------------------
	// SetSteerAngle: Sets steering angle (radians)
	//------------------------------------------------------------------------
	public void SetSteerAngle(float stAngle)
	{
		
		steerFrame.Rotation = new Vector3(0.0f, stAngle, 0.0f);
	}

	//------------------------------------------------------------------------
	// Setters:
	//------------------------------------------------------------------------
	public float WheelRadius
	{
		set
		{
			if(value > 0.0f)
				wheelRad = value;
		}
	}

	public float SteeredWheelRadius
	{
		set{
			if(value > 0.0f)
				wheelRadS = value;
		}
	}

	public float WheelSeparation
	{
		set{
			if(value > 0.1f)
				wheelSep = value;
		}
	}

	public float BaseLength
	{
		set{
			if(value > 0.1f)
				baseLen = value;
		}
	}

	public float CasterLength
	{
		set{
			if(value > 0.0f)
				casterLen = value;
		}
	}

	public float TireWidth
	{
		set{
			if(value > 0.0f)
				tireWd = value;
		}
	}

	public float WheelWidth
	{
		set{
			if(value > 0.0f)
				wheelWd = value;
		}
	}

	public float SteeredTireWidth
	{
		set{
			if(value > 0.0f)
				tireWdS = value;
		}
	}

	public float SteeredWheelWidth
	{
		set{
			if(value > 0.0f)
				wheelWdS = value;
		}
	}

	public float FrameBarWidth
	{
		set{
			if(value > 0.0f)
				barWidth = value;
		}
	}

	public float SteerAngle
	{
		set{
			SetSteerAngle(value);
		}
	}
}
