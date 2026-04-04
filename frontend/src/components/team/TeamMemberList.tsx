import { Button } from "@/components/ui/button";
import { TeamMemberDto } from "@/types";

type TeamMemberListProps = {
  members: TeamMemberDto[];
};

export default function TeamMemberList({ members }: TeamMemberListProps) {
  return (
    <ul className="space-y-4">
      {members.map((member) => (
        <li key={member.userId} className="flex justify-between items-center p-4 bg-white rounded shadow">
          <div>
            <p className="font-medium">{member.displayName}</p>
            <p className="text-sm text-gray-500">{member.email}</p>
            <p className="text-sm text-gray-600">{member.role}</p>
          </div>
          <Button variant="ghost" aria-label="Remove Member">
            Remove
          </Button>
        </li>
      ))}
    </ul>
  );
}