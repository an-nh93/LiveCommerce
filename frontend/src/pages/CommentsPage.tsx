import { useEffect, useState, useRef } from 'react';
import { Card, Table, Select, Space, Typography, Drawer, Button, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import * as signalR from '@microsoft/signalr';
import { commentsApi, liveSessionsApi, type CommentListDto, type CommentDetailDto, type LiveSessionDto } from '../api/client';
import { useAuthStore } from '../stores/authStore';

const COMMENT_STATUS: Record<number, string> = {
  0: 'Mới',
  1: 'Đã giao',
  2: 'Đang xử lý',
  3: 'Đã đặt hàng',
  4: 'Cần follow-up',
  5: 'Bỏ qua',
  6: 'Hủy',
};

const API_BASE = import.meta.env.VITE_API_URL || '';

export function CommentsPage() {
  const token = useAuthStore((s) => s.accessToken);
  const [sessions, setSessions] = useState<LiveSessionDto[]>([]);
  const [comments, setComments] = useState<CommentListDto[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [liveSessionId, setLiveSessionId] = useState<number | undefined>();
  const [status, setStatus] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<CommentDetailDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
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

  const loadDetail = async (id: number) => {
    setDetailLoading(true);
    try {
      const { data } = await commentsApi.getById(id);
      if (data.success && data.data) setSelected(data.data);
      else setSelected(null);
    } catch {
      setSelected(null);
    } finally {
      setDetailLoading(false);
    }
  };

  useEffect(() => {
    loadSessions();
  }, []);

  useEffect(() => {
    loadComments();
  }, [liveSessionId, status, page]);

  // SignalR: connect and join live session group
  useEffect(() => {
    if (!token || !liveSessionId) return;
    const url = `${API_BASE}/hubs/comments`;
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();
    conn.on('CommentUpdated', () => {
      loadComments();
      if (selected) loadDetail(selected.id);
    });
    conn.start().then(() => conn.invoke('JoinLiveSession', liveSessionId)).catch(() => {});
    connectionRef.current = conn;
    return () => {
      conn.invoke('LeaveLiveSession', liveSessionId).catch(() => {});
      conn.stop();
      connectionRef.current = null;
    };
  }, [token, liveSessionId]);

  const handleTake = async () => {
    if (!selected) return;
    setActionLoading(true);
    try {
      const { data } = await commentsApi.take(selected.id);
      if (data.success && data.data) {
        message.success('Đã nhận comment');
        loadComments();
        loadDetail(selected.id);
      } else message.error(data.message || 'Thất bại');
    } catch {
      message.error('Lỗi');
    } finally {
      setActionLoading(false);
    }
  };

  const handleStatus = async (newStatus: number) => {
    if (!selected) return;
    setActionLoading(true);
    try {
      const { data } = await commentsApi.updateStatus(selected.id, newStatus);
      if (data.success && data.data) {
        message.success('Đã cập nhật trạng thái');
        loadComments();
        setSelected(data.data as CommentDetailDto);
      } else message.error(data.message || 'Thất bại');
    } catch {
      message.error('Lỗi');
    } finally {
      setActionLoading(false);
    }
  };

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
          onRow={(r) => ({ onClick: () => loadDetail(r.id), style: { cursor: 'pointer' } })}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: false,
            onChange: setPage,
          }}
        />
      </Card>
      <Drawer
        title="Chi tiết comment"
        open={!!selected}
        onClose={() => setSelected(null)}
        width={400}
        loading={detailLoading}
        footer={
          selected ? (
            <Space>
              {selected.status === 0 || selected.status === 1 ? (
                <Button type="primary" loading={actionLoading} onClick={handleTake}>
                  Nhận xử lý
                </Button>
              ) : null}
              {selected.status === 1 || selected.status === 2 ? (
                <>
                  <Button loading={actionLoading} onClick={() => handleStatus(2)}>Đang xử lý</Button>
                  <Button loading={actionLoading} onClick={() => handleStatus(3)}>Đã đặt hàng</Button>
                  <Button loading={actionLoading} onClick={() => handleStatus(4)}>Cần follow-up</Button>
                  <Button loading={actionLoading} onClick={() => handleStatus(5)}>Bỏ qua</Button>
                </>
              ) : null}
            </Space>
          ) : null
        }
      >
        {selected && (
          <>
            <p><strong>Nội dung:</strong> {selected.content}</p>
            <p><strong>Người gửi:</strong> {selected.senderName || selected.senderExternalId || '-'}</p>
            <p><strong>Trạng thái:</strong> {COMMENT_STATUS[selected.status] ?? selected.status}</p>
            <p><strong>Người xử lý:</strong> {selected.assignedUserName || '-'}</p>
            {(selected.customerName || selected.customerPhone) && (
              <p><strong>Khách hàng:</strong> {selected.customerName} {selected.customerPhone}</p>
            )}
          </>
        )}
      </Drawer>
    </div>
  );
}
