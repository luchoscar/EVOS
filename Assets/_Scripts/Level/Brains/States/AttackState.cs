
using UnityEngine;

namespace Game2D
{
    public class AttackState : IBrainState
    {
        public CharacterActionState State => CharacterActionState.Attack;

        public CharacterActionState[] NextState => new[]
        {
            CharacterActionState.FreeMovement
        };

        private UnitMovement _unitMovement = UnitMovement.Idle;
        private bool _attackingPlayer = false;

        public bool TryEnterState(NPCController controller)
        {
            _attackingPlayer = (controller.IsDetectingPlayer() && controller.PlayerWithInAttackRange());
            if (_attackingPlayer)
            {
                _unitMovement = MoveToPlayer(controller);
            }

            return _attackingPlayer;
        }

        public void ProcessInput(NPCController controller, ref InputData inputData)
        {
            int horizontalInput = (_unitMovement & UnitMovement.MoveRight) == UnitMovement.MoveRight
                ? 1
                : (_unitMovement & UnitMovement.MoveLeft) == UnitMovement.MoveLeft
                    ? -1
                    : 0;
            inputData.SetHorizontal(horizontalInput);
        }

        public bool TryExitState(NPCController controller)
        {
            return !_attackingPlayer && !controller.PlayerWithInAttackRange();
        }

        private UnitMovement MoveToPlayer(NPCController controller)
        {
            Vector3 playerPosition = controller.Player.GetTransform().position;
            Vector3 nodeDirection = (playerPosition - controller.transform.position).normalized;
            float dotProd = Vector3.Dot(nodeDirection, controller.transform.forward);

            return dotProd > 0
                ? UnitMovement.MoveRight
                : dotProd < 0
                    ? UnitMovement.MoveLeft
                    : UnitMovement.Idle;
        }

        public void ProcessAttackAnimation(NPCController controller, AnimationEvent animationEvent)
        {
            if (animationEvent.stringParameter == AnimationEventKey.End)
            {
                _attackingPlayer = false;
            } else if (animationEvent.stringParameter == AnimationEventKey.Attack
                 && _unitMovement == MoveToPlayer(controller)
                 && controller.IsDetectingPlayer()
                 && controller.PlayerWithInAttackRange()
                )
            {
                controller.Player.TakeDamage(controller.AttackDamage, controller);
            }
        }
    }
}