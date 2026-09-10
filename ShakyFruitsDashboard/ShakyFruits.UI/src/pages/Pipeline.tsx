import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { JobsTable } from "@/components/jobs/JobsTable";
import { ListVideo } from "lucide-react";
import { useState } from "react";
import { PublishModal } from "@/components/pipeline/PublishModal";

export function Pipeline() {
  const [publishModalOpen, setPublishModalOpen] = useState(false);
  const [selectedPublishJob, setSelectedPublishJob] = useState<any>(null);

  const handleOpenPublish = (job: any) => {
    setSelectedPublishJob(job);
    setPublishModalOpen(true);
  };
  
  return (
    <div className="space-y-6 animate-in fade-in duration-300">
      
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h2 className="text-2xl font-semibold tracking-tight flex items-center gap-2">
            <ListVideo className="w-6 h-6 text-primary" />
            Production Pipeline
          </h2>
          <p className="text-muted-foreground">
            Manage video generation queues, review outputs, and track rendering statuses.
          </p>
        </div>
      </div>

      <Card className="shadow-sm border-border/40">
        <CardHeader className="bg-muted/20 border-b border-border/40 pb-4">
          <CardTitle className="text-xl">Production Pipeline</CardTitle>
          <CardDescription>Real-time generation tasks tracked by the local C# backend.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {/* handleOpenPublish fonksiyonunu tabloya prop olarak gönderiyoruz */}
          <JobsTable onPublish={handleOpenPublish} />
        </CardContent>
      </Card>

      {/* Yayınlama Modalı */}
      <PublishModal 
        isOpen={publishModalOpen}
        onClose={() => {
          setPublishModalOpen(false);
          setSelectedPublishJob(null);
        }}
        videoGenerationId={selectedPublishJob?.id}
        videoPath={selectedPublishJob?.outputVideoPath}
        videoTitle={selectedPublishJob?.title}
        defaultCaption={selectedPublishJob?.aiGeneratedCaption}
      />

    </div>
  );
}