import { apiFetch } from "@/lib/api-client";
import { useQuery } from "@tanstack/react-query";
import { SubscriptionDto, PlanDto } from "@/types";

export function useSubscription() {
  const {
    data: subscription,
    error: subscriptionError,
    isLoading: subscriptionLoading,
  } = useQuery<SubscriptionDto>({
    queryKey: ["subscription"],
    queryFn: () => apiFetch<SubscriptionDto>("/api/v1/billing/subscription"),
  });

  const {
    data: plans,
    error: plansError,
    isLoading: plansLoading,
  } = useQuery<PlanDto[]>({
    queryKey: ["plans"],
    queryFn: () => apiFetch<PlanDto[]>("/api/v1/billing/plans"),
  });

  return {
    subscription,
    plans: plans ?? [],
    error: subscriptionError || plansError,
    loading: subscriptionLoading || plansLoading,
  };
}