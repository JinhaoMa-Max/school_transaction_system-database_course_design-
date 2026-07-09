import request from '@/utils/request'

//上传结果：图像和url
interface UploadImageResult {
  fileName: string
  url: string
}

export const uploadImage = (file: File) => {
  const formData = new FormData()
  formData.append('file', file)

  return request.post<UploadImageResult>('/upload/image', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}