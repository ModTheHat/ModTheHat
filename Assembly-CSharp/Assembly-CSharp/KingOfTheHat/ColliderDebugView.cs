// Decompiled with JetBrains decompiler
// Type: ColliderDebugView
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7A049970-C314-4BB9-8469-3CD036A16C96
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\King of the Hat\KingOfTheHat_Data\Managed\Assembly-CSharp.dll

using BCI.Loops;
using Determinism;
using Entitas;
using ModTheHat;
using UnityEngine;

public class ColliderDebugView : BCIUpdateBehaviour
{
    public bool showColliders = true;
    public bool showFollowPoints = true;
    public bool showPickupRadius = true;
    private LogicContext _context;
    private IGroup<LogicEntity> _allColliders;
    private IGroup<LogicEntity> _allFollowPoints;
    

    public extern void orig_Initialize(UIState state);
    protected override void Initialize()
    {
        base.Initialize();
        this._context = Contexts.sharedInstance.logic;
        this._allColliders = this._context.GetGroup(LogicMatcher.Collider);
        this._allFollowPoints = this._context.GetGroup(LogicMatcher.FollowPoint);
        this.showColliders = true; // Modified to Enable
        this.showFollowPoints = true; // Modified to Enable
        this.showPickupRadius = true; // Modified to Enable
        this.enabled = true;
    }

    public override void Execute(float alpha)
    {
        base.Execute(alpha);
        foreach (LogicEntity allCollider in this._allColliders)
        {
            if (this.showColliders)
            {
                Color color1 = new Color(1f, 0.0f, 1f, 1f);
                if (allCollider.isDangerous)
                    color1 = Color.red;
                else if (allCollider.hasPlayerID || allCollider.hasFollowPoint)
                    color1 = Color.green;
                FixedVector2 fixedVector2;
                foreach (FixedLine2 edge in allCollider.collider.value.edges)
                {
                    fixedVector2 = edge.start;
                    Vector3 vector3_1 = fixedVector2.ToVector3();
                    fixedVector2 = edge.end;
                    Vector3 vector3_2 = fixedVector2.ToVector3();
                    Color color2 = color1;
                    GameDebug.Gizmos.DrawLine(vector3_1, vector3_2, color2);
                }
                if (allCollider.hasPlayerID)
                {
                    foreach (FixedLine2 edge in allCollider.collider.value.edges)
                    {
                        fixedVector2 = edge.start;
                        Vector3 vector3_3 = fixedVector2.ToVector3();
                        fixedVector2 = edge.end;
                        Vector3 vector3_4 = fixedVector2.ToVector3();
                        Color color3 = color1;
                        GameDebug.Gizmos.DrawLine(vector3_3, vector3_4, color3);
                    }
                }
            }
        }
        foreach (LogicEntity allFollowPoint in this._allFollowPoints)
        {
            if (this.showFollowPoints)
            {
                FixedVector2 fixedVector2 = GameController.Find(allFollowPoint.followPoint.targetID).position.value;
                this.DrawPoint(fixedVector2.ToVector3());
                if (this.showPickupRadius)
                {
                    long pickUpRadius = allFollowPoint.followPoint.pickUpRadius;
                    GameDebug.Gizmos.DrawCircle(fixedVector2.ToVector3(), pickUpRadius.ToFloat() * 2f, Color.blue);
                }
            }
        }
        if (!Release.Flag.CHRIS_BOUNDING_BOX_KILL)
            return;
        foreach (LogicEntity allCollider in this._allColliders)
        {
            if (allCollider.hasPlayerID)
            {
                BoundingBox boundingBox = new BoundingBox();
                boundingBox.Resize(new FixedVector2(allCollider.collider.value.bounds.size.x, PlayerTriggerCommands.KillBoxHeight));
                long num = -allCollider.collider.value.bounds.halfExtents.y + boundingBox.halfExtents.y;
                boundingBox.position = new FixedVector2(allCollider.collider.value.position.x, allCollider.collider.value.position.y + num);
                GameDebug.Gizmos.DrawSquare(boundingBox.position.ToVector3(), boundingBox.size.ToVector3(), Color.red);
            }
            if (allCollider.IsHat())
            {
                BoundingBox boundingBox = new BoundingBox();
                boundingBox.Resize(new FixedVector2(allCollider.collider.value.bounds.size.x, PlayerTriggerCommands.KillBoxHeight));
                long num = allCollider.collider.value.halfExtents.y - boundingBox.halfExtents.y;
                boundingBox.position = new FixedVector2(allCollider.collider.value.position.x, allCollider.collider.value.position.y + num);
                GameDebug.Gizmos.DrawSquare(boundingBox.position.ToVector3(), boundingBox.size.ToVector3(), Color.red);
            }
        }
    }

    public override void FixedExecute() => base.FixedExecute();

    private void DrawPoint(Vector3 point) => GameDebug.Gizmos.DrawCircle(point, 0.25f, Color.blue);
}
