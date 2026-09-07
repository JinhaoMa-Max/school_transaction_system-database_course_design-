import request from '@/utils/request'

/** 获取帮助手册（markdown 原文） */
export const getManual = () => request.get<string>('/help/manual')
