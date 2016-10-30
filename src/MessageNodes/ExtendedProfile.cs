using VVVV.Packs.Messaging;

namespace VVVV.Nodes.Messaging
{
/// <summary>
/// All classes extending TypeIdentity will be consumed at startup. Make sure the assembly is really loaded.
/// </summary>
    public class VVVVProfile : TypeIdentity
    {
        protected override void Register(TypeIdentity target = null)
        {
            if (target == null) target = this;

            target.TryAddRecord(new TypeRecord<VVVV.Utils.VColor.RGBAColor>("Color", CloneBehaviour.Assign, () => new VVVV.Utils.VColor.RGBAColor()));
            target.TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Matrix4x4>("Transform", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Matrix4x4()));
            target.TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Vector2D>("Vector2d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector2D()));
            target.TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Vector3D>("Vector3d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector3D()));
            target.TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Vector4D>("Vector4d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector4D()));

/*            target.TryAddRecord(new TypeRecord<DX11.DX11Resource<VVVV.DX11.DX11Layer>>("Layer", CloneBehaviour.Assign));
            target.TryAddRecord(new TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.IDX11ReadableStructureBuffer>>("Buffer", CloneBehaviour.Assign));
            target.TryAddRecord(new TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.DX11Texture2D>>("Texture2d", CloneBehaviour.Assign));
            target.TryAddRecord(new TypeRecord<DX11.DX11Resource<FeralTic.DX11.Resources.IDX11Geometry>>("Geometry", CloneBehaviour.Assign));
*/
        }

    }
}
