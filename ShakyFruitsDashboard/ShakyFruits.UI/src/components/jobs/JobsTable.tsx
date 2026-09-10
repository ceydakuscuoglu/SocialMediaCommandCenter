import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import {
  FileImage,
  FileVideo,
  Loader2,
  AlertCircle,
  RefreshCw,
  Sparkles,
  Calendar,
  FolderOpen,
  TextQuote,
  Trash2,
  Send,
  Pencil
} from "lucide-react";
import { getFileName, formatDate } from "@/utils/formatters";
import { JobStatusBadge } from "./JobStatusBadge";
import { useJobs } from "@/hooks/useJobs";
import { Command } from '@tauri-apps/plugin-shell';
import { deleteJob } from "@/api/jobs.api";
import { useState } from "react";
import { UpdateJobModal } from "@/components/pipeline/UpdateJobModal";

interface JobsTableProps {
  onPublish?: (job: any) => void;
}

export function JobsTable({ onPublish }: JobsTableProps = {}) {
  const { data: jobs, isLoading, isError, error } = useJobs();
  const [updateModalOpen, setUpdateModalOpen] = useState(false);
  const [selectedUpdateJob, setSelectedUpdateJob] = useState<any>(null);

  const handleOpenFolder = async (path: string | null) => {
    if (!path) return;
    try {
      const cleanPath = path.replace(/["']/g, "");
      const folderPath = cleanPath.substring(0, Math.max(cleanPath.lastIndexOf('\\'), cleanPath.lastIndexOf('/'))) || cleanPath;
      const safePath = folderPath.replace(/\//g, '\\');

      console.log("Shell Command'e Giden Yol:", safePath);
      const command = Command.create('run-explorer', [safePath]);
      await command.spawn();
    } catch (err) {
      console.error("Shell Command Hatası:", err);
      alert(`Tauri Hatası:\n${err}`);
    }
  }

  const handleDelete = async (jobId: number) => {
    try {
      await deleteJob(jobId);
      window.location.reload();
    } catch (err) {
      console.error("Silme Hatası:", err);
      alert(`Hata:\n${err instanceof Error ? err.message : err}`);
    }
  };

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
        <Loader2 className="w-8 h-8 animate-spin text-primary mb-4" />
        <p>Loading production pipeline...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-destructive">
        <AlertCircle className="w-8 h-8 mb-4" />
        <p className="font-medium">API Connection Error</p>
        <p className="text-sm opacity-80">{error instanceof Error ? error.message : "Unknown error occurred"}</p>
      </div>
    );
  }

  if (!jobs || jobs.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-muted-foreground">
        <p>No generation jobs found in the database.</p>
      </div>
    );
  }

  return (
    <>
      <Table>
        <TableHeader className="bg-muted/10">
          <TableRow className="hover:bg-transparent whitespace-nowrap">
            <TableHead className="w-[5%] text-center">ID</TableHead>
            {/* Title için alan genişletildi (%15'ten %25'e) */}
            <TableHead className="w-[25%]">Title</TableHead>
            <TableHead className="w-[15%]">Assets</TableHead>
            <TableHead className="w-[10%] text-center">Prompt</TableHead>
            <TableHead className="w-[10%] text-center">Job Type</TableHead>
            <TableHead className="w-[10%] text-center">Status</TableHead>
            {/* Created At genişliği dengelendi */}
            <TableHead className="w-[10%] text-center">Created At</TableHead>
            <TableHead className="w-[15%] pr-6 text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {jobs.map((job) => (
            <TableRow key={job.id} className="group transition-colors hover:bg-muted/50">
              {/* 1. ID */}
              <TableCell className="font-medium text-center">#{job.id}</TableCell>

              {/* 2. SADECE TITLE - truncate kısıtlamaları kaldırıldı, tam görünüm eklendi */}
              <TableCell className="py-3">
                {job.title ? (
                  <div className="font-semibold text-foreground text-sm tracking-tight whitespace-normal break-words min-w-[180px]">
                    {job.title}
                  </div>
                ) : (
                  <div className="text-muted-foreground/60 italic text-sm">
                    Untitled
                  </div>
                )}
              </TableCell>

              {/* 3. SADECE ASSETS */}
              <TableCell className="py-3">
                <div className="flex flex-col gap-1.5">
                  <div className="flex items-center gap-2 text-xs text-muted-foreground w-full">
                    <FileImage className="w-3.5 h-3.5 shrink-0 text-emerald-500/70" />
                    <span className="truncate max-w-[150px] lg:max-w-[200px]" title={job.fruitImagePath}>
                      {getFileName(job.fruitImagePath)}
                    </span>
                  </div>
                  <div className="flex items-center gap-2 text-xs text-muted-foreground w-full">
                    <FileVideo className="w-3.5 h-3.5 shrink-0 text-sky-500/70" />
                    <span className="truncate max-w-[150px] lg:max-w-[200px]" title={job.referenceVideoPath}>
                      {getFileName(job.referenceVideoPath)}
                    </span>
                  </div>
                </div>
              </TableCell>

              {/* 4. Prompt Butonu */}
              <TableCell className="text-center">
                <Dialog>
                  <DialogTrigger asChild>
                    <Button variant="secondary" size="sm" className="h-8 gap-2 text-muted-foreground hover:text-foreground">
                      <TextQuote className="w-3.5 h-3.5" />
                      View
                    </Button>
                  </DialogTrigger>
                  <DialogContent className="sm:max-w-[600px]">
                    <DialogHeader>
                      <DialogTitle className="flex items-center gap-2">
                        <TextQuote className="w-5 h-5 text-primary" />
                        Applied Motion Prompt
                      </DialogTitle>
                    </DialogHeader>
                    <div className="bg-muted p-4 rounded-md text-sm text-foreground leading-relaxed">
                      {job.appliedPrompt}
                    </div>
                  </DialogContent>
                </Dialog>
              </TableCell>

              {/* 5. Job Type Rozeti */}
              <TableCell className="text-center">
                {job.isRecreate ? (
                  <Badge variant="outline" className="text-amber-500 border-amber-500/20 bg-amber-500/5">
                    <RefreshCw className="w-3 h-3 mr-1" /> Recreate
                  </Badge>
                ) : (
                  <Badge variant="outline" className="text-primary border-primary/20 bg-primary/5">
                    <Sparkles className="w-3 h-3 mr-1" /> New Job
                  </Badge>
                )}
              </TableCell>

              {/* 6. Status Rozeti */}
              <TableCell>
                <div className="flex justify-center">
                  <JobStatusBadge status={job.status} errorMessage={job.errorMessage} />
                </div>
              </TableCell>

              {/* 7. Tarih */}
              <TableCell className="text-muted-foreground whitespace-nowrap text-sm">
                <div className="flex items-center justify-center gap-1.5">
                  <Calendar className="w-3.5 h-3.5 opacity-70" />
                  {formatDate(job.createdAt)}
                </div>
              </TableCell>

              {/* 8. Actions */}
              <TableCell className="pr-6 text-right">
                <div className="flex items-center justify-end gap-2">
                  {(String(job.status).toLowerCase() === "completed" || String(job.status) === "2") && onPublish && (
                    <Button
                      variant="default"
                      size="sm"
                      className="h-8 gap-1.5 bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm"
                      onClick={() => onPublish(job)}
                      title="Publish to Social Media"
                    >
                      <Send className="w-3.5 h-3.5" /> Publish
                    </Button>
                  )}

                  {(String(job.status).toLowerCase() === "completed" || String(job.status) === "2") && job.outputVideoPath && (
                    <Button
                      variant="secondary"
                      size="icon"
                      className="h-8 w-8 bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20"
                      title="Open Output Video"
                      onClick={() => handleOpenFolder(job.outputVideoPath)}
                    >
                      <FolderOpen className="w-4 h-4" />
                    </Button>
                  )}

                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-amber-500 hover:bg-amber-500/10 hover:text-amber-600"
                    title="Edit Job"
                    onClick={() => {
                      setSelectedUpdateJob(job);
                      setUpdateModalOpen(true);
                    }}
                  >
                    <Pencil className="w-4 h-4" />
                  </Button>

                  <AlertDialog>
                    <AlertDialogTrigger asChild>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8 text-destructive hover:bg-destructive/10 hover:text-destructive"
                        title="Delete Job"
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                        <AlertDialogDescription>
                          This action cannot be undone. This will permanently delete Job #{job.id}{job.title ? ` ("${job.title}")` : ""} from the database and remove associated assets.
                        </AlertDialogDescription>
                      </AlertDialogHeader>
                      <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction
                          onClick={() => handleDelete(job.id)}
                          className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                        >
                          Delete
                        </AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <UpdateJobModal
        isOpen={updateModalOpen}
        onClose={() => {
          setUpdateModalOpen(false);
          setSelectedUpdateJob(null);
        }}
        job={selectedUpdateJob}
      />
    </>
  );
}