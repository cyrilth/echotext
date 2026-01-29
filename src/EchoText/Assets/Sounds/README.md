# Sound Effects

This directory should contain the following sound effect files:

- `start.wav` - Played when recording starts (RecordingStart)
- `stop.wav` - Played when recording stops (RecordingStop)
- `success.wav` - Played when transcription completes successfully (Success)
- `error.wav` - Played when an error occurs (Error)

## Specifications

All sound files should be:
- Format: WAV (PCM)
- Duration: 0.1-0.5 seconds (short and non-intrusive)
- Sample Rate: 44100 Hz or 48000 Hz
- Channels: Mono or Stereo
- Bit Depth: 16-bit

## Current Implementation

Until actual sound files are added, the NotificationService uses system beeps as placeholders:
- RecordingStart: Short high beep (800 Hz, 100ms)
- RecordingStop: Short low beep (400 Hz, 100ms)
- Success: Two short high beeps (800 Hz + 1000 Hz)
- Error: Long low beep (300 Hz, 300ms)

## Obtaining Sound Files

You can:
1. Create custom sound effects using audio software (Audacity, etc.)
2. Use royalty-free sound effects from sites like freesound.org
3. Generate simple beeps programmatically
4. Record your own sounds

Make sure any sounds you use are properly licensed for your use case.
