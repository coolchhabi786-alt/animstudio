import { redirect } from "next/navigation";

interface Props {
  params: { id: string; episodeId: string };
}

export default function EpisodeDetailPage({ params }: Props) {
  redirect(`/projects/${params.id}/episodes/${params.episodeId}/script`);
}
