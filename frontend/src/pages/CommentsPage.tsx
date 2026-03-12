import { useEffect, useState } from 'react';
import { Card, Table, Select, Space, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { commentsApi, liveSessionsApi, type CommentListDto, type LiveSessionDto } from '../api/client';

const COMMENT_STATUS: Record<number, string> = {
  0: 'Mới',
  1: 'Đã giao',
  2: 'Đang xử lý',
  3: 'Đã đặt hàng',
  4: 'Cần follow-up',
  5: 'Bỏ qua',
  6: 'Hủy',
};

export function CommentsPage() {
  const [sessions, setSessions] = useState<LiveSessionDto[]>([]);
  const [comments, setComments] = useState<CommentListDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [liveSessionId, setLiveSessionId] = useState<number | undefined>();
  const [status, setStatus] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const loadSessions = async () => {
    try {
      const { data } = await liveSessionsApi.list();
      if (data.success && data.data) setSessions(data.data);
    } catch {
      setSessions([]);
    }
  };

  const loadComments = async () => {
    setLoading(true);
    try {
      const { data } = await commentsApi.list({
        liveSessionId,
        status,
        page,
        pageSize,
      });
      if (data.success && data.data) {
        setComments(data.data.items);
        setTotal(data.data.totalCount);
      } else {
        setComments([]);
        setTotal(0);
      }
    } catch {
      setComments([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSessions();
  }, []);

  useEffect(() => {
    loadComments();
  }, [liveSessionId, status, page]);

  const columns: ColumnsType<CommentListDto> = [
    { title: 'Thời gian', dataIndex: 'commentTimeUtc', key: 'commentTimeUtc', width: 180, render: (v: string) => new Date(v).toLocaleString() },
    { title: 'Nội dung', dataIndex: 'content', key: 'content', ellipsis: true },
    { title: 'Người gửi', dataIndex: 'senderName', key: 'senderName', width: 120 },
    { title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 120, render: (s: number) => COMMENT_STATUS[s] ?? s },
    { title: 'Người xử lý', dataIndex: 'assignedUserName', key: 'assignedUserName', width: 120 },
  ];

  return (
    <div>
      <h2>Comment Center</h2>
      <Card>
        <Space wrap style={{ marginBottom: 16 }}>
          <Typography.Text>Phiên livestream:</Typography.Text>
          <Select
            placeholder="Tất cả phiên"
            allowClear
            style={{ width: 220 }}
            value={liveSessionId}
            onChange={setLiveSessionId}
            options={[{ value: undefined, label: 'Tất cả' }, ...sessions.map((s) => ({ value: s.id, label: s.name }))]}
          />
          <Typography.Text>Trạng thái:</Typography.Text>
          <Select
            placeholder="Tất cả"
            allowClear
            style={{ width: 160 }}
            value={status}
            onChange={setStatus}
            options={[
              { value: undefined, label: 'Tất cả' },
              ...Object.entries(COMMENT_STATUS).map(([k, v]) => ({ value: Number(k), label: v })),
            ]}
          />
        </Space>
        <Table
          rowKey="id"
          columns={columns}
          dataSource={comments}
          loading={loading}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: false,
            onChange: setPage,
          }}
        />
      </Card>
    </div>
  );
}
