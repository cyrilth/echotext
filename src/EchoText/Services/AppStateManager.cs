using System;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Manages the application's state machine with thread-safe state transitions
/// </summary>
/// <remarks>
/// Valid state transitions:
/// - Loading → Idle
/// - Idle → Recording
/// - Recording → Processing
/// - Recording → Idle
/// - Processing → Idle
/// - Processing → Error
/// - Any state → Error
/// - Error → Idle
/// </remarks>
public class AppStateManager : IAppStateManager
{
    private readonly object _lock = new();
    private AppState _currentState = AppState.Loading;

    /// <inheritdoc/>
    public AppState CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _currentState;
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler<AppState>? StateChanged;

    /// <inheritdoc/>
    public bool TransitionTo(AppState newState)
    {
        lock (_lock)
        {
            // Check if transition is valid
            if (!IsValidTransition(_currentState, newState))
            {
                return false;
            }

            // Perform the transition
            var oldState = _currentState;
            _currentState = newState;

            // Fire event outside of lock to prevent potential deadlocks
            var handler = StateChanged;
            if (handler != null)
            {
                // Fire event asynchronously to not block the calling thread
                Task.Run(() => handler(this, newState));
            }

            return true;
        }
    }

    /// <inheritdoc/>
    public bool CanTransitionTo(AppState newState)
    {
        lock (_lock)
        {
            return IsValidTransition(_currentState, newState);
        }
    }

    /// <summary>
    /// Validates if a state transition is allowed
    /// </summary>
    private static bool IsValidTransition(AppState current, AppState target)
    {
        // Same state transitions are not allowed
        if (current == target)
        {
            return false;
        }

        // Any state can transition to Error
        if (target == AppState.Error)
        {
            return true;
        }

        // Define valid transitions based on the state machine
        return (current, target) switch
        {
            // From Loading
            (AppState.Loading, AppState.Idle) => true,

            // From Idle
            (AppState.Idle, AppState.Recording) => true,

            // From Recording
            (AppState.Recording, AppState.Processing) => true,
            (AppState.Recording, AppState.Idle) => true,

            // From Processing
            (AppState.Processing, AppState.Idle) => true,

            // From Error
            (AppState.Error, AppState.Idle) => true,

            // All other transitions are invalid
            _ => false
        };
    }
}
