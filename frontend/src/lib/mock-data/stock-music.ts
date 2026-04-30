export interface StockMusicTrack {
  id: string;
  title: string;
  durationSeconds: number;
  genre: string;
  mood: string;
  previewUrl: string;
  fullUrl: string;
}

const SH = "https://www.soundhelix.com/examples/mp3";

export const STOCK_MUSIC_TRACKS: StockMusicTrack[] = [
  {
    id: "stock-m-01",
    title: "Neon Horizon",
    durationSeconds: 180,
    genre: "Ambient",
    mood: "Atmospheric",
    previewUrl: `${SH}/SoundHelix-Song-1.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-1.mp3`,
  },
  {
    id: "stock-m-02",
    title: "Epic Ascent",
    durationSeconds: 210,
    genre: "Epic",
    mood: "Powerful",
    previewUrl: `${SH}/SoundHelix-Song-2.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-2.mp3`,
  },
  {
    id: "stock-m-03",
    title: "Morning Light",
    durationSeconds: 195,
    genre: "Uplifting",
    mood: "Happy",
    previewUrl: `${SH}/SoundHelix-Song-3.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-3.mp3`,
  },
  {
    id: "stock-m-04",
    title: "Shadow Protocol",
    durationSeconds: 240,
    genre: "Suspense",
    mood: "Tense",
    previewUrl: `${SH}/SoundHelix-Song-4.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-4.mp3`,
  },
  {
    id: "stock-m-05",
    title: "Rubber Duck Parade",
    durationSeconds: 170,
    genre: "Comedy",
    mood: "Playful",
    previewUrl: `${SH}/SoundHelix-Song-5.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-5.mp3`,
  },
  {
    id: "stock-m-06",
    title: "Deep Focus",
    durationSeconds: 300,
    genre: "Ambient",
    mood: "Calm",
    previewUrl: `${SH}/SoundHelix-Song-6.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-6.mp3`,
  },
  {
    id: "stock-m-07",
    title: "City Pulse",
    durationSeconds: 200,
    genre: "Electronic",
    mood: "Energetic",
    previewUrl: `${SH}/SoundHelix-Song-7.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-7.mp3`,
  },
  {
    id: "stock-m-08",
    title: "Gentle Rain",
    durationSeconds: 220,
    genre: "Ambient",
    mood: "Relaxing",
    previewUrl: `${SH}/SoundHelix-Song-8.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-8.mp3`,
  },
  {
    id: "stock-m-09",
    title: "Victory March",
    durationSeconds: 185,
    genre: "Epic",
    mood: "Triumphant",
    previewUrl: `${SH}/SoundHelix-Song-9.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-9.mp3`,
  },
  {
    id: "stock-m-10",
    title: "Desert Wind",
    durationSeconds: 250,
    genre: "World",
    mood: "Mysterious",
    previewUrl: `${SH}/SoundHelix-Song-10.mp3`,
    fullUrl: `${SH}/SoundHelix-Song-10.mp3`,
  },
];
