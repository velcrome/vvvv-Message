namespace VVVV.Packs.Messaging.Nodes
{
/// <summary>
/// All classes implementing TypeProfile will be consumed at startup. Make sure the assembly is really loaded.
/// </summary>
    public class VVVVProfile : TypeProfile
    {
        public TypeRecord<VVVV.Utils.VColor.RGBAColor> Color = new TypeRecord<VVVV.Utils.VColor.RGBAColor>("Color", CloneBehaviour.Assign, () => new VVVV.Utils.VColor.RGBAColor());
        public TypeRecord<VVVV.Utils.VMath.Matrix4x4> Transform = new TypeRecord<VVVV.Utils.VMath.Matrix4x4>("Transform", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Matrix4x4());
        public TypeRecord<VVVV.Utils.VMath.Vector2D> Vector2d = new TypeRecord<VVVV.Utils.VMath.Vector2D>("Vector2d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector2D());
        public TypeRecord<VVVV.Utils.VMath.Vector3D> Vector3d = new TypeRecord<VVVV.Utils.VMath.Vector3D>("Vector3d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector3D());
        public TypeRecord<VVVV.Utils.VMath.Vector4D> Vector4d = new TypeRecord<VVVV.Utils.VMath.Vector4D>("Vector4d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector4D());

        public TypeRecord<DX11.DX11Resource<VVVV.DX11.DX11Layer>> Layer = new TypeRecord<DX11.DX11Resource<VVVV.DX11.DX11Layer>>("Layer", CloneBehaviour.Null, () => null);
        public TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.IDX11ReadableStructureBuffer>> Buffer = new TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.IDX11ReadableStructureBuffer>>("Buffer", CloneBehaviour.Null, () => null);
        public TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.DX11Texture2D>> Texture2d = new TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.DX11Texture2D>>("Texture2d", CloneBehaviour.Null, () => null);
        public TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.IDX11Geometry>> Geometry = new TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.IDX11Geometry>>("Geometry", CloneBehaviour.Null, () => null);

    }
}
