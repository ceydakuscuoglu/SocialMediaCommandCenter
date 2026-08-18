import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { FileImage, FileVideo, Loader2, AlertCircle, RefreshCw, Sparkles, Calendar } from "lucide-react";
import { getFileName, formatDate } from "@/utils/formatters"; // formatDate import edildi
import { JobStatusBadge } from "./JobStatusBadge";
import { useJobs } from "@/hooks/useJobs";

export function JobsTable() {
    const { data: jobs, isLoading, isError, error } = useJobs();

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
        <Table>
            <TableHeader className="bg-muted/10">
                <TableRow className="hover:bg-transparent">
                    <TableHead className="w-[80px] pl-6">ID</TableHead>
                    <TableHead>Source Assets</TableHead>
                    <TableHead className="hidden md:table-cell w-[30%]">Applied Prompt</TableHead>
                    <TableHead>Job Type</TableHead>
                    <TableHead className="text-right pr-6">Created At</TableHead> {/* Yeni Başlık */}
                    <TableHead>Status</TableHead>
                </TableRow>
            </TableHeader>
            <TableBody>
                {jobs.map((job) => (
                    <TableRow key={job.id} className="group transition-colors hover:bg-muted/50">
                        <TableCell className="font-medium pl-6">#{job.id}</TableCell>

                        <TableCell className="flex flex-col gap-1.5 py-3">
                            <div className="flex items-center gap-2 text-sm">
                                <FileImage className="w-4 h-4 text-muted-foreground" />
                                <span className="truncate max-w-[150px]" title={job.fruitImagePath}>
                                    {getFileName(job.fruitImagePath)}
                                </span>
                            </div>
                            <div className="flex items-center gap-2 text-sm">
                                <FileVideo className="w-4 h-4 text-muted-foreground" />
                                <span className="truncate max-w-[150px]" title={job.referenceVideoPath}>
                                    {getFileName(job.referenceVideoPath)}
                                </span>
                            </div>
                        </TableCell>

                        <TableCell className="hidden md:table-cell text-muted-foreground text-sm">
                            <p className="line-clamp-2" title={job.appliedPrompt}>
                                {job.appliedPrompt}
                            </p>
                        </TableCell>

                        <TableCell>
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


                        {/* Yeni Tarih Sütunu */}
                        <TableCell className="text-right text-muted-foreground pr-6 whitespace-nowrap text-sm">
                            <div className="flex items-center justify-end gap-1.5">
                                <Calendar className="w-3.5 h-3.5 opacity-70" />
                                {formatDate(job.createdAt)}
                            </div>
                        </TableCell>
                        <TableCell>
                            <JobStatusBadge status={job.status} />
                        </TableCell>

                    </TableRow>
                ))}
            </TableBody>
        </Table>
    );
}