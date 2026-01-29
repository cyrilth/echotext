using EchoText.Models;
using EchoText.Services;
using FluentAssertions;
using Xunit;

namespace EchoText.Tests.Services;

public class AppStateManagerTests
{
    [Fact]
    public void Constructor_ShouldInitializeToLoadingState()
    {
        // Arrange & Act
        var stateManager = new AppStateManager();

        // Assert
        stateManager.CurrentState.Should().Be(AppState.Loading);
    }

    [Fact]
    public void TransitionTo_FromLoadingToIdle_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();
        AppState? eventState = null;
        stateManager.StateChanged += (_, state) => eventState = state;

        // Act
        var result = stateManager.TransitionTo(AppState.Idle);

        // Assert
        result.Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Idle);

        // Wait briefly for async event
        Task.Delay(50).Wait();
        eventState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void TransitionTo_FromIdleToRecording_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);

        // Act
        var result = stateManager.TransitionTo(AppState.Recording);

        // Assert
        result.Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Recording);
    }

    [Fact]
    public void TransitionTo_FromRecordingToProcessing_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Recording);

        // Act
        var result = stateManager.TransitionTo(AppState.Processing);

        // Assert
        result.Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Processing);
    }

    [Fact]
    public void TransitionTo_FromRecordingToIdle_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Recording);

        // Act
        var result = stateManager.TransitionTo(AppState.Idle);

        // Assert
        result.Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void TransitionTo_FromProcessingToIdle_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Recording);
        stateManager.TransitionTo(AppState.Processing);

        // Act
        var result = stateManager.TransitionTo(AppState.Idle);

        // Assert
        result.Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void TransitionTo_FromErrorToIdle_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Error);

        // Act
        var result = stateManager.TransitionTo(AppState.Idle);

        // Assert
        result.Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void TransitionTo_AnyStateToError_ShouldSucceed()
    {
        // Arrange
        var stateManager = new AppStateManager();

        // Act & Assert - From Loading
        stateManager.TransitionTo(AppState.Error).Should().BeTrue();
        stateManager.CurrentState.Should().Be(AppState.Error);

        // From Idle
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Error).Should().BeTrue();

        // From Recording
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Recording);
        stateManager.TransitionTo(AppState.Error).Should().BeTrue();

        // From Processing
        stateManager.TransitionTo(AppState.Idle);
        stateManager.TransitionTo(AppState.Recording);
        stateManager.TransitionTo(AppState.Processing);
        stateManager.TransitionTo(AppState.Error).Should().BeTrue();
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ShouldFail()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        var originalState = stateManager.CurrentState;

        // Act - Try to go from Idle directly to Processing
        var result = stateManager.TransitionTo(AppState.Processing);

        // Assert
        result.Should().BeFalse();
        stateManager.CurrentState.Should().Be(originalState);
    }

    [Fact]
    public void TransitionTo_SameState_ShouldFail()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);

        // Act
        var result = stateManager.TransitionTo(AppState.Idle);

        // Assert
        result.Should().BeFalse();
        stateManager.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void TransitionTo_FromLoadingToRecording_ShouldFail()
    {
        // Arrange
        var stateManager = new AppStateManager();

        // Act
        var result = stateManager.TransitionTo(AppState.Recording);

        // Assert
        result.Should().BeFalse();
        stateManager.CurrentState.Should().Be(AppState.Loading);
    }

    [Fact]
    public void TransitionTo_FromIdleToProcessing_ShouldFail()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);

        // Act
        var result = stateManager.TransitionTo(AppState.Processing);

        // Assert
        result.Should().BeFalse();
        stateManager.CurrentState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void CanTransitionTo_ValidTransition_ShouldReturnTrue()
    {
        // Arrange
        var stateManager = new AppStateManager();

        // Act & Assert
        stateManager.CanTransitionTo(AppState.Idle).Should().BeTrue();
        stateManager.CanTransitionTo(AppState.Error).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_InvalidTransition_ShouldReturnFalse()
    {
        // Arrange
        var stateManager = new AppStateManager();

        // Act & Assert
        stateManager.CanTransitionTo(AppState.Recording).Should().BeFalse();
        stateManager.CanTransitionTo(AppState.Processing).Should().BeFalse();
        stateManager.CanTransitionTo(AppState.Loading).Should().BeFalse();
    }

    [Fact]
    public void CanTransitionTo_SameState_ShouldReturnFalse()
    {
        // Arrange
        var stateManager = new AppStateManager();

        // Act & Assert
        stateManager.CanTransitionTo(AppState.Loading).Should().BeFalse();

        stateManager.TransitionTo(AppState.Idle);
        stateManager.CanTransitionTo(AppState.Idle).Should().BeFalse();
    }

    [Fact]
    public void StateChanged_ShouldFireOnTransition()
    {
        // Arrange
        var stateManager = new AppStateManager();
        var eventFired = false;
        AppState? receivedState = null;

        stateManager.StateChanged += (sender, state) =>
        {
            eventFired = true;
            receivedState = state;
        };

        // Act
        stateManager.TransitionTo(AppState.Idle);

        // Wait for async event
        Task.Delay(50).Wait();

        // Assert
        eventFired.Should().BeTrue();
        receivedState.Should().Be(AppState.Idle);
    }

    [Fact]
    public void StateChanged_ShouldNotFireOnFailedTransition()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);

        var eventFired = false;
        stateManager.StateChanged += (_, _) => eventFired = true;

        // Act
        stateManager.TransitionTo(AppState.Processing); // Invalid transition

        // Wait to ensure event doesn't fire
        Task.Delay(50).Wait();

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    public void CurrentState_IsThreadSafe()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        var states = new List<AppState>();
        var tasks = new List<Task>();

        // Act - Multiple threads reading state concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    lock (states)
                    {
                        states.Add(stateManager.CurrentState);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert - All reads should succeed without exception
        states.Should().HaveCount(1000);
        states.Should().OnlyContain(state => state == AppState.Idle);
    }

    [Fact]
    public void TransitionTo_IsThreadSafe()
    {
        // Arrange
        var stateManager = new AppStateManager();
        stateManager.TransitionTo(AppState.Idle);
        var tasks = new List<Task>();
        var successCount = 0;

        // Act - Multiple threads attempting transitions concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                if (stateManager.TransitionTo(AppState.Recording))
                {
                    Interlocked.Increment(ref successCount);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert - Only one transition should succeed
        successCount.Should().Be(1);
        stateManager.CurrentState.Should().Be(AppState.Recording);
    }

    [Theory]
    [InlineData(AppState.Loading, AppState.Idle, true)]
    [InlineData(AppState.Loading, AppState.Recording, false)]
    [InlineData(AppState.Loading, AppState.Processing, false)]
    [InlineData(AppState.Idle, AppState.Recording, true)]
    [InlineData(AppState.Idle, AppState.Processing, false)]
    [InlineData(AppState.Recording, AppState.Processing, true)]
    [InlineData(AppState.Recording, AppState.Idle, true)]
    [InlineData(AppState.Processing, AppState.Idle, true)]
    [InlineData(AppState.Processing, AppState.Recording, false)]
    [InlineData(AppState.Error, AppState.Idle, true)]
    [InlineData(AppState.Error, AppState.Recording, false)]
    public void TransitionTo_VariousScenarios_ShouldMatchExpected(
        AppState fromState, AppState toState, bool expectedResult)
    {
        // Arrange
        var stateManager = new AppStateManager();

        // Transition to the starting state
        switch (fromState)
        {
            case AppState.Loading:
                // Already in Loading state
                break;
            case AppState.Idle:
                stateManager.TransitionTo(AppState.Idle);
                break;
            case AppState.Recording:
                stateManager.TransitionTo(AppState.Idle);
                stateManager.TransitionTo(AppState.Recording);
                break;
            case AppState.Processing:
                stateManager.TransitionTo(AppState.Idle);
                stateManager.TransitionTo(AppState.Recording);
                stateManager.TransitionTo(AppState.Processing);
                break;
            case AppState.Error:
                stateManager.TransitionTo(AppState.Error);
                break;
        }

        // Act
        var result = stateManager.TransitionTo(toState);

        // Assert
        result.Should().Be(expectedResult);
        if (expectedResult)
        {
            stateManager.CurrentState.Should().Be(toState);
        }
        else
        {
            stateManager.CurrentState.Should().Be(fromState);
        }
    }
}
