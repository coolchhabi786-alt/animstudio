src/components/storyboard/style-override-popover.tsx
Props: { shot: StoryboardShot, onApply: (style: string) => void }

Display: Popover with 6 style buttons
Presets: "Realistic", "Cartoon", "Anime", "Watercolor", "Pencil Sketch", "3D Render"
Visual: Color-coded buttons
Feedback: "Style applied! Regenerating..." toast
what is the purpose of this presets in here.are we going to regenate the image based on this present? Are we going to send a new request to the Fal.ai for the new imgae generation or we going to do this edit here only withoutb making any AI call and if so how we are going to do that?


Compacted conversationGreat architectural question! Let me clarify the design intent here.

## Purpose of Style Presets

The **style presets are for regenerating the storyboard shot** with a different visual aesthetic **without regenerating the entire animation**. Here's the distinction:

- **Initial Generation**: Fal.ai (or Kling) generates the entire animation clip (5-8 seconds) in "default" style
- **Style Override**: User re-renders just that **shot's image** (a single frame/still) with a different style applied

---

## Architecture: Two Options

### **Option A: Backend Regeneration (Recommended)**
When user clicks "Cartoon":
1. Frontend sends: `{ shotId, newStyle: "Cartoon" }`
2. Backend calls animation service with style parameter:
   ```
   Fal.ai inpaint or re-render endpoint with new style prompt
   e.g., "A cartoon-style scene of..."
   ```
3. New image generated → Returns to frontend
4. Shot image updates (storyboard thumbnail changes)

**Cost**: ~$0.02-0.03 per regeneration (cheaper than full animation)

**Timeline**: 5-10 seconds per regeneration

---

### **Option B: UI-Only (Not Recommended - but possible for mock)**
When user clicks "Watercolor":
1. Apply CSS filters locally (no backend call)
   ```css
   filter: sepia(0.5) saturate(1.2) blur(0.5px);
   mix-blend-mode: screen;
   ```
2. Shows instant preview
3. **Problem**: Doesn't actually change the rendered output → Misleading

---

## For Phase 6 (What You're Building Now)

Since Phase 6 uses **mock data**, here's what the mock implementation should do:

```typescript
// Mock behavior (Phase 6 - Frontend only)
const handleStyleApply = (style: string) => {
  showToast("Style applied! Regenerating...")
  
  // Mock API call simulation
  setTimeout(() => {
    // Simulate backend regeneration
    const newImageUrl = mockStoryboard.shots[selectedShotIndex].imageUrl
    // In real app: newImageUrl would come from backend
    
    updateShot({
      ...currentShot,
      imageUrl: newImageUrl, // Changes thumbnail
      styleOverride: style,
      regenerationCount: currentShot.regenerationCount + 1
    })
    
    showToast("New style applied! ✓", "success")
  }, 2000) // Simulate 2-second regeneration time
}
```

**In mock Phase 6**: Just update the UI state + show toast. The image stays the same in the mock, but the `regenerationCount` increments to show it "regenerated."

---

## For Real Implementation (After Phase 10 - Backend)

You'd add a real API call:

```typescript
// Real implementation (after backend ready)
async function regenerateShot(shotId: string, newStyle: string) {
  const response = await fetch("/api/storyboard/regenerate", {
    method: "POST",
    body: JSON.stringify({ shotId, style: newStyle })
  })
  
  const { imageUrl, costUsd } = await response.json()
  return { imageUrl, costUsd }
}
```

The backend would:
- Parse the original shot prompt
- Append style: `"[Original prompt], in a [Cartoon] style"`
- Call Fal.ai/Kling API
- Return new image URL + cost

---

## Summary for Your Phase 6 Component

| Aspect | Phase 6 (Mock) | Real Implementation |
|--------|---|---|
| **When user clicks style** | Show toast, increment counter | Show toast, call backend |
| **API call** | None (mock only) | POST `/api/storyboard/regenerate` |
| **Image update** | Mock (same image, UI state changes) | Real (new image from backend) |
| **Cost** | $0 (mock) | ~$0.02-0.03 per regeneration |
| **Time** | 2-second mock delay | 5-10 seconds real regeneration |

**Your Phase 6 code should include a comment noting**: 
```typescript
// TODO: Replace mock timeout with real API call to POST /api/storyboard/regenerate
// Backend will call Fal.ai with new style parameter
```

Does this clarify the architecture? Want me to show you the actual Phase 6 component code with proper mock implementation?


For the stage 7 we have the option of the voice-picker.tsx now my question how do we get the list of all voices available in the Azure TTS and also all the supported language

Also what is this feature of Voice cloning how does it work in the Azure TTS as currently the core engine in python which is main pipeline for the animation generation is not supporting that feature and what is the cost of this voice cloning feature

Optimizing tool selection...Excellent architecture questions! Let me break these down:

---

## 1. Getting Azure TTS Voices & Languages

### Option A: Static List (Recommended for Phase 7)
Microsoft publishes a static JSON list of all available voices:

```typescript
// src/lib/azure-tts/voices.ts
export const azureVoices = [
  {
    locale: "en-US",
    name: "en-US-AriaNeural",
    displayName: "Aria",
    gender: "Female",
    previewUrl: "https://..."
  },
  {
    locale: "en-US",
    name: "en-US-GuyNeural",
    displayName: "Guy",
    gender: "Male",
    previewUrl: "https://..."
  },
  // 400+ more voices...
]

export const supportedLanguages = [
  { code: "en-US", name: "English (US)" },
  { code: "es-ES", name: "Spanish (Spain)" },
  { code: "fr-FR", name: "French (France)" },
  // 140+ languages...
]
```

**Where to get the list:**
1. Download from: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-support?tabs=tts
2. Parse Azure's voice endpoint: `https://{region}.tts.speech.microsoft.com/cognitiveservices/v1`
3. Call Azure REST API to list voices dynamically

### Option B: Dynamic API Call (For real implementation)
```typescript
async function getAvailableVoices(region: string) {
  const response = await fetch(
    `https://${region}.tts.speech.microsoft.com/cognitiveservices/voices/list`,
    {
      headers: {
        'Ocp-Apim-Subscription-Key': AZURE_API_KEY
      }
    }
  )
  return response.json() // Returns 400+ voices
}
```

---

## 2. Voice Cloning in Azure TTS

### What It Actually Is:

**Azure has THREE tiers of voice capabilities:**

| Feature | Free | Standard | Premium (Studio) |
|---------|------|----------|------------------|
| **Pre-built Neural Voices** | ✅ Yes (20 voices) | ✅ Yes (400+ voices) | ✅ Yes (400+ voices) |
| **Custom Neural Voice** | ❌ No | ❌ No | ✅ Yes (Training) |
| **Personal Voice** (Preview) | ❌ No | ❌ No | ✅ Yes (Cloning) |
| **Cost** | $0 | $1-4 per 1M chars | $10-50/month + $0.30 per cloned voice |

---

### How Voice Cloning Works in Azure:

**Personal Voice** (newer, preview mode):
1. User records 3 minutes of audio samples (~10 short sentences)
2. Uploads to Azure Cognitive Services
3. Azure trains a custom voice model (24-48 hours)
4. Model becomes available as a voice choice
5. User can then synthesize speech in their own voice

**Custom Neural Voice** (older, enterprise):
1. Upload 300+ minutes of high-quality audio
2. Provide phonetic transcriptions
3. Submit for approval (Microsoft reviews quality)
4. If approved: Training takes weeks
5. Custom voice available for synthesis

---

## 3. Does Your Python Pipeline Support Voice Cloning?

**Probably NO** if you're using:
- `gTTS` (Google Text-to-Speech) → No cloning
- `pyttsx3` (offline TTS) → No cloning
- Basic Azure TTS endpoint → No cloning
- OpenAI Whisper (speech-to-text) → Wrong tool, doesn't generate speech

**To add voice cloning to Python pipeline:**
```python
# You'd need to:
1. Use Azure Cognitive Services SDK (Python)
2. Upload training samples via API
3. Wait 24-48 hours for model training
4. Call synthesis endpoint with custom voice ID

# Example (pseudocode):
from azure.cognitiveservices.speech import SpeechConfig
config = SpeechConfig(endpoint=endpoint, auth_token=token)
config.voice_name = "personalVoice_john"  # Custom trained voice
synthesizer = SpeechSynthesizer(speech_config=config)
audio = synthesizer.speak_text("Hello from John's voice")
```

---

## 4. Cost Breakdown for Voice Cloning

| Component | Cost |
|-----------|------|
| **Azure Personal Voice Setup** | $0 (preview/included) |
| **Per Cloned Voice Model** | $0.30 per voice (one-time) |
| **Azure Studio Tier** | $25-50/month |
| **Synthesis after cloning** | Standard rates ($1-4 per 1M chars) |
| **Training time** | 24-48 hours (free) |
| **Total for 1 cloned voice** | ~$30-50/month + $0.30 setup |

### Comparison to Stock Voices:
```
Stock Voice (Aria, Guy):
- Cost: $1.50 per 1M characters
- 1 episode (3 min dialogue): ~$0.01-0.02

Cloned Voice (Character's voice):
- Cost: $50/month + $1.50 per 1M characters
- 1 episode: ~$50.01-50.02 (dominated by monthly fee)
```

**Verdict**: **Only worth it for Studio tier subscribers** wanting branded character voices.

---

## Recommendation for Phase 7

### **For Phase 7 (Mock Implementation):**

1. **Use stock Azure voices** (free tier has 20, standard has 400+)
2. **Mock the voice cloning feature** (show UI, but it's tier-locked)
3. **Store voice choice** in database: `{ characterId, voiceName: "en-US-AriaNeural" }`

### **Component Structure:**

```typescript
// Phase 7: Voice Studio

// Stock voices - use these
export const STOCK_VOICES = [
  { name: "en-US-AriaNeural", displayName: "Aria", gender: "Female" },
  { name: "en-US-GuyNeural", displayName: "Guy", gender: "Male" },
  // ... 18+ more
]

// Voice cloning - tier-locked in UI
export const VOICE_CLONING = {
  isAvailable: false, // Unless subscribed to Studio
  tierRequired: "Studio",
  cost: "$0.30 per voice + $50/month",
  setupTime: "24-48 hours",
  maxCharacterVoices: 10
}
```

### **Mock Implementation:**

```typescript
// src/components/voice/voice-clone-upload.tsx
export function VoiceCloneUpload({ characterId, isTierLocked }) {
  if (isTierLocked) {
    return (
      <div className="border-2 border-dashed border-gray-300 rounded p-6">
        <Lock className="h-8 w-8 text-gray-400" />
        <p className="text-sm text-gray-600">
          Voice cloning unavailable
        </p>
        <p className="text-xs text-gray-500 mt-2">
          Studio tier required ($50/month)
        </p>
        <Button variant="outline" className="mt-4">
          Upgrade to Studio
        </Button>
      </div>
    )
  }

  return (
    <div className="border-2 border-dashed border-blue-300 rounded p-6">
      <Upload className="h-8 w-8 text-blue-500" />
      <input type="file" accept="audio/*" />
      <p className="text-sm text-gray-600 mt-2">
        Upload 3 minutes of your voice samples
      </p>
      <p className="text-xs text-gray-500 mt-1">
        Processing time: 24-48 hours
      </p>
      <Button className="mt-4">Upload Voice Samples</Button>
    </div>
  )
}
```

---

## Architecture Decision

For **Phase 7 (Voice Studio)**, I recommend:

| Approach | Best For |
|----------|----------|
| **Use stock Azure voices only** | Now (Phase 7) + Free tier launch |
| **Add voice cloning UI (tier-locked)** | After Studio tier ready (Phase 12) |
| **Implement backend voice cloning** | After Phase 12 (separate microservice) |

---

## Your Python Pipeline

**Current**: Uses basic TTS (no cloning support) → ✅ Correct for animation generation
**Future**: Could add voice cloning for Studio tier → Requires separate workflow:
1. User uploads voice samples via frontend (Phase 7)
2. Backend triggers Azure voice cloning API
3. Wait 24-48 hours
4. When ready → Animation pipeline uses custom voice for that episode

---

**For Phase 7, should we:**
1. **Only implement stock voice picker** (simpler mock)?
2. **Add UI for voice cloning (tier-locked)** to show the full feature?
3. **Do you want code for both approaches?**

Which direction? 🎙️ 🎨