using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;

namespace ItemIcons.Agents;

[StructLayout(LayoutKind.Explicit, Size = 0x8)]
public unsafe partial struct AtkDragDropInterface
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct AtkDragDropInterfaceVTable
    {
        [FieldOffset(0)] public delegate* unmanaged[Stdcall]<AtkDragDropInterface*, bool, void> Destroy;
        [FieldOffset(8)] public delegate* unmanaged[Stdcall]<AtkDragDropInterface*, float*, float*, void> GetScreenPosition;
        [FieldOffset(56)] public delegate* unmanaged[Stdcall]<AtkDragDropInterface*, AtkComponentBase*> GetComponent;
    }

    [FieldOffset(0x0)] public AtkDragDropInterfaceVTable* VTable;

    public void Destroy(bool free)
    {
        fixed (AtkDragDropInterface* thisPtr = &this)
        {
            VTable->Destroy(thisPtr, free);
        }
    }

    public void GetScreenPosition(float* screenX, float* screenY)
    {
        fixed (AtkDragDropInterface* thisPtr = &this)
        {
            VTable->GetScreenPosition(thisPtr, screenX, screenY);
        }
    }

    public AtkComponentBase* GetComponent()
    {
        fixed (AtkDragDropInterface* thisPtr = &this)
        {
            return VTable->GetComponent(thisPtr);
        }
    }
}
