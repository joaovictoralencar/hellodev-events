# HelloDev Events

ScriptableObject-based event system for decoupled communication. Enables components to communicate without direct references.

## Features

- **GameEventBase_SO** - Abstract base class for all event types
- **GameEvent_SO\<T\>** - Generic typed event base for creating custom event types
- **GameEventVoid_SO** - Parameterless event for simple notifications
- **Typed Events** - Built-in: GameEventBool_SO, GameEventInt_SO, GameEventFloat_SO, GameEventString_SO
- **Auto-reset** - Listeners automatically clear between play sessions via RuntimeScriptableObject
- **Duplicate Prevention** - AddListener prevents duplicate subscriptions (O(1) with HashSet)
- **LastValue** - Access the most recent raised value for late subscribers
- **HasListeners** - Quick check if any listeners are subscribed
- **SafeSubscribe/SafeUnsubscribe** - Convenience methods matching Utils extension patterns
- **Custom Editor** - Inspector shows listener count and debug options

## Getting Started

### 1. Install the Package

**Via Package Manager (Local):**
1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" > "Add package from disk"
3. Navigate to this folder and select `package.json`

**Dependencies:** Ensure `com.hellodev.utils` is installed.

### 2. Create Your First Event

1. Right-click in Project window
2. Select **Create > HelloDev > Events > Int Game Event**
3. Name it descriptively (e.g., `OnScoreChanged`)

### 3. Wire Up a Publisher

```csharp
using HelloDev.Events;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private GameEventInt_SO onScoreChanged;

    private int _score;

    public void AddScore(int points)
    {
        _score += points;
        onScoreChanged.Raise(_score);  // Notify all listeners
    }
}
```

### 4. Wire Up a Subscriber

```csharp
using HelloDev.Events;
using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private GameEventInt_SO onScoreChanged;
    [SerializeField] private TMP_Text scoreText;

    void OnEnable()
    {
        onScoreChanged.AddListener(UpdateDisplay);

        // Catch up with current value if event was already raised
        if (onScoreChanged.HasBeenRaised)
        {
            UpdateDisplay(onScoreChanged.LastValue);
        }
    }

    void OnDisable()
    {
        onScoreChanged.RemoveListener(UpdateDisplay);
    }

    void UpdateDisplay(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}
```

### 5. Assign the Same Event Asset to Both

In the Unity Inspector, drag the same `OnScoreChanged` event asset to both the `ScoreManager` and `ScoreDisplay` components. Now they communicate without knowing about each other!

## Installation

### Via Package Manager (Local)
1. Open Unity Package Manager
2. Click "+" > "Add package from disk"
3. Navigate to this folder and select `package.json`

## Usage

### Creating Events

```
Assets > Create > HelloDev > Events > Game Event (Void)
Assets > Create > HelloDev > Events > Int Game Event
Assets > Create > HelloDev > Events > Bool Game Event
etc.
```

### Subscribing to Events

```csharp
using HelloDev.Events;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private GameEventInt_SO onScoreChanged;

    void OnEnable() => onScoreChanged.AddListener(UpdateScore);
    void OnDisable() => onScoreChanged.RemoveListener(UpdateScore);

    void UpdateScore(int score) => scoreText.text = score.ToString();
}
```

### Using Void Events

```csharp
[SerializeField] private GameEventVoid_SO onGameStart;
[SerializeField] private GameEventVoid_SO onPause;

void OnEnable()
{
    onGameStart.AddListener(HandleGameStart);
    onPause.AddListener(HandlePause);
}

void OnDisable()
{
    onGameStart.RemoveListener(HandleGameStart);
    onPause.RemoveListener(HandlePause);
}

void HandleGameStart() => Debug.Log("Game started!");
void HandlePause() => Time.timeScale = 0;
```

### Raising Events

```csharp
[SerializeField] private GameEventInt_SO onScoreChanged;
[SerializeField] private GameEventVoid_SO onLevelComplete;

public void AddPoints(int points)
{
    score += points;
    onScoreChanged.Raise(score);
}

public void CompleteLevel()
{
    onLevelComplete.Raise();  // No parameter needed
}
```

### Using LastValue for Late Subscribers

```csharp
void OnEnable()
{
    onScoreChanged.AddListener(UpdateScore);

    // Catch up with current value if event was already raised
    if (onScoreChanged.HasBeenRaised)
    {
        UpdateScore(onScoreChanged.LastValue);
    }
}
```

### Using SafeSubscribe

```csharp
// Removes any existing subscription before adding - prevents duplicates
onScoreChanged.SafeSubscribe(UpdateScore);

// Null-safe unsubscribe
onScoreChanged.SafeUnsubscribe(UpdateScore);
```

### Creating Custom Event Types

```csharp
using HelloDev.Events;
using UnityEngine;

[CreateAssetMenu(menuName = "HelloDev/Events/Vector3 Game Event")]
public class GameEventVector3_SO : GameEvent_SO<Vector3> { }
```

### Creating ID-Based Events

For the generic event pattern (recommended for quest systems):

```csharp
using HelloDev.Events;
using HelloDev.IDs;
using UnityEngine;

[CreateAssetMenu(menuName = "HelloDev/Events/ID Game Event")]
public class GameEventID_SO : GameEvent_SO<ID_SO> { }
```

Usage:
```csharp
// Monster death handler
[SerializeField] private ID_SO monsterId;
[SerializeField] private GameEventID_SO onMonsterKilled;

void OnDeath()
{
    onMonsterKilled.Raise(monsterId);  // Same event, different ID
}
```

## API Reference

### GameEventBase_SO
| Member | Description |
|--------|-------------|
| `ListenerCount` | Number of subscribed listeners |
| `HasListeners` | True if any listeners are subscribed |
| `ParameterType` | Type of event parameter (null for void) |
| `RemoveAllListeners()` | Clears all subscriptions |

### GameEvent_SO\<T\>
| Member | Description |
|--------|-------------|
| `LastValue` | Most recent value passed to Raise |
| `HasBeenRaised` | True if Raise called since reset |
| `AddListener(callback)` | Subscribe (duplicates ignored) |
| `RemoveListener(callback)` | Unsubscribe |
| `SafeSubscribe(callback)` | Remove + Add (prevents duplicates) |
| `SafeUnsubscribe(callback)` | Null-safe unsubscribe |
| `Raise(value)` | Invoke all listeners with value |

### GameEventVoid_SO
| Member | Description |
|--------|-------------|
| `HasBeenRaised` | True if Raise called since reset |
| `AddListener(callback)` | Subscribe (duplicates ignored) |
| `RemoveListener(callback)` | Unsubscribe |
| `SafeSubscribe(callback)` | Remove + Add (prevents duplicates) |
| `SafeUnsubscribe(callback)` | Null-safe unsubscribe |
| `Raise()` | Invoke all listeners |

## Dependencies

- com.hellodev.utils (1.1.0+)

## Changelog

### v1.1.0 (2025-12-21)
**Performance:**
- Listener tracking now uses HashSet for O(1) add/remove/contains (was O(n) List)
- Separate List maintained only for editor display

**New Features:**
- Added `GameEventVoid_SO` for parameterless events (OnGameStart, OnPause, etc.)
- Added `LastValue` property to get most recent raised value
- Added `HasBeenRaised` property to check if event was raised since reset
- Added `HasListeners` property on base class for quick existence check
- Added `SafeSubscribe()` and `SafeUnsubscribe()` convenience methods

**Documentation:**
- Added XML documentation to all public classes and methods

**Package:**
- Updated Unity version to 6000.3
- Updated utils dependency to 1.1.0

### v1.0.0
- Initial release

## License

MIT License
