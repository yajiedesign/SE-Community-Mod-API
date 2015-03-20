namespace SEModAPI.Interop
{
    public enum CubeType
    {
        None,
        Exterior,
        Interior,
        Cube,
        // TODO: split the cube type definition from the orientation of that cube type.
        SlopeCenterFrontTop,
        SlopeLeftFrontCenter,
        SlopeRightFrontCenter,
        SlopeCenterFrontBottom,
        SlopeLeftCenterTop,
        SlopeRightCenterTop,
        SlopeLeftCenterBottom,
        SlopeRightCenterBottom,
        SlopeCenterBackTop,
        SlopeLeftBackCenter,
        SlopeRightBackCenter,
        SlopeCenterBackBottom,
        NormalCornerLeftFrontTop,
        NormalCornerRightFrontTop,
        NormalCornerLeftBackTop,
        NormalCornerRightBackTop,
        NormalCornerLeftFrontBottom,
        NormalCornerRightFrontBottom,
        NormalCornerLeftBackBottom,
        NormalCornerRightBackBottom,
        InverseCornerLeftFrontTop,
        InverseCornerRightFrontTop,
        InverseCornerLeftBackTop,
        InverseCornerRightBackTop,
        InverseCornerLeftFrontBottom,
        InverseCornerRightFrontBottom,
        InverseCornerLeftBackBottom,
        InverseCornerRightBackBottom,

        // These are actually to represent generic orientations for any cube.
        Axis24_Backward_Down,
        Axis24_Backward_Left,
        Axis24_Backward_Right,
        Axis24_Backward_Up,
        Axis24_Down_Backward,
        Axis24_Down_Forward,
        Axis24_Down_Left,
        Axis24_Down_Right,
        Axis24_Forward_Down,
        Axis24_Forward_Left,
        Axis24_Forward_Right,
        Axis24_Forward_Up,
        Axis24_Left_Backward,
        Axis24_Left_Down,
        Axis24_Left_Forward,
        Axis24_Left_Up,
        Axis24_Right_Backward,
        Axis24_Right_Down,
        Axis24_Right_Forward,
        Axis24_Right_Up,
        Axis24_Up_Backward,
        Axis24_Up_Forward,
        Axis24_Up_Left,
        Axis24_Up_Right,
    };
}
