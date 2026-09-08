"""校园二手交易平台测试。

脚本只执行读取和登录验证，不会创建、修改或删除业务数据。
仅使用 Python 标准库，无须安装第三方依赖。
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from dataclasses import dataclass
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


@dataclass
class Response:
    status: int
    body: bytes

    @property
    def text(self) -> str:
        return self.body.decode("utf-8", errors="replace")


class Tester:
    def __init__(self, base_url: str, timeout: float) -> None:
        self.base_url = base_url.rstrip("/")
        self.timeout = timeout
        self.passed = 0
        self.failed = 0

    def request(
        self,
        path: str,
        *,
        method: str = "GET",
        json_body: dict[str, Any] | None = None,
        token: str | None = None,
    ) -> Response:
        headers = {"Accept": "application/json, text/html, */*"}
        data = None

        if json_body is not None:
            data = json.dumps(json_body, ensure_ascii=False).encode("utf-8")
            headers["Content-Type"] = "application/json"

        if token:
            headers["Authorization"] = f"Bearer {token}"

        request = Request(
            f"{self.base_url}{path}",
            data=data,
            headers=headers,
            method=method,
        )

        try:
            with urlopen(request, timeout=self.timeout) as response:
                return Response(response.status, response.read())
        except HTTPError as error:
            return Response(error.code, error.read())
        except (URLError, TimeoutError, OSError) as error:
            return Response(0, str(error).encode("utf-8", errors="replace"))

    def pass_test(self, name: str) -> None:
        self.passed += 1
        print(f"[PASS] {name}")

    def fail_test(self, name: str, detail: str) -> None:
        self.failed += 1
        print(f"[FAIL] {name}：{detail}")

    def check(
        self,
        name: str,
        response: Response,
        expected_status: int,
        expected_text: str | None = None,
    ) -> bool:
        if response.status != expected_status:
            excerpt = response.text.replace("\r", " ").replace("\n", " ")[:180]
            self.fail_test(
                name,
                f"期望 HTTP {expected_status}，实际 HTTP {response.status}；{excerpt or '响应为空'}",
            )
            return False

        if expected_text is not None and expected_text not in response.text:
            excerpt = response.text.replace("\r", " ").replace("\n", " ")[:180]
            self.fail_test(
                name,
                f"HTTP 状态正确，但响应缺少 {expected_text!r}；{excerpt or '响应为空'}",
            )
            return False

        self.pass_test(name)
        return True


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="校园二手交易平台测试")
    parser.add_argument(
        "--base-url",
        default=os.getenv("BASE_URL", "http://127.0.0.1"),
        help="网站地址（默认：环境变量 BASE_URL 或 http://127.0.0.1）",
    )
    parser.add_argument(
        "--account",
        default=os.getenv("TEST_ACCOUNT", "admin"),
        help="测试账号（默认：admin）",
    )
    parser.add_argument(
        "--password",
        default=os.getenv("TEST_PASSWORD", "pass123"),
        help="测试密码（默认：pass123）",
    )
    parser.add_argument(
        "--goods-id",
        type=int,
        default=int(os.getenv("TEST_GOODS_ID", "1")),
        help="用于详情测试的商品 ID（默认：1）",
    )
    parser.add_argument(
        "--image-path",
        default=os.getenv("TEST_IMAGE_PATH", "/uploads/1/iphone15_front.jpg"),
        help="用于静态图片测试的 URL 路径",
    )
    parser.add_argument(
        "--timeout",
        type=float,
        default=float(os.getenv("TEST_TIMEOUT", "20")),
        help="单次请求超时秒数（默认：20）",
    )
    return parser.parse_args()


def extract_token(response: Response) -> str:
    try:
        payload = json.loads(response.text)
    except json.JSONDecodeError:
        return ""

    token = payload.get("token")
    if isinstance(token, str) and token:
        return token

    data = payload.get("data")
    if isinstance(data, dict) and isinstance(data.get("token"), str):
        return data["token"]

    return ""


def main() -> int:
    args = parse_args()
    tester = Tester(args.base_url, args.timeout)

    print("校园二手交易平台测试")
    print(f"测试地址：{tester.base_url}")
    print("-" * 40)

    response = tester.request("/")
    tester.check("前端首页可访问", response, 200, '<div id="app"></div>')

    response = tester.request(f"/goods/{args.goods_id}")
    tester.check("前端详情路由可直接访问", response, 200, '<div id="app"></div>')

    response = tester.request("/api/health")
    tester.check("后端健康检查", response, 200, "backend is running")

    response = tester.request("/api/goods?page=1&size=8&status=approved")
    tester.check("商品列表及数据库查询", response, 200, '"list"')

    response = tester.request(f"/api/goods/{args.goods_id}")
    tester.check("商品详情接口", response, 200, f'"goodsId":{args.goods_id}')

    response = tester.request(f"/api/goods/{args.goods_id}/images")
    tester.check("商品图片记录接口", response, 200, '"imageUrl"')

    response = tester.request(args.image_path)
    if response.status == 200 and response.body:
        tester.pass_test("商品静态图片可访问")
    else:
        tester.fail_test(
            "商品静态图片可访问",
            f"期望 HTTP 200 且文件非空，实际 HTTP {response.status}",
        )

    response = tester.request("/api/auth/current")
    tester.check("未登录访问受保护接口被拒绝", response, 401, '"code":401')

    response = tester.request(
        "/api/auth/login",
        method="POST",
        json_body={"account": args.account, "password": "test-wrong-password"},
    )
    tester.check("错误密码被拒绝", response, 401, '"code":401')

    login_response = tester.request(
        "/api/auth/login",
        method="POST",
        json_body={"account": args.account, "password": args.password},
    )
    tester.check("正确账号可以登录", login_response, 200, '"token"')

    token = extract_token(login_response) if login_response.status == 200 else ""
    if token:
        response = tester.request("/api/auth/current", token=token)
        tester.check(
            "登录令牌可以读取当前用户",
            response,
            200,
            f'"username":"{args.account}"',
        )
    else:
        tester.fail_test("登录令牌可以读取当前用户", "登录响应中没有解析到 token")

    print("-" * 40)
    total = tester.passed + tester.failed
    print(f"测试完成：通过 {tester.passed}，失败 {tester.failed}，总计 {total}")
    return 1 if tester.failed else 0


if __name__ == "__main__":
    sys.exit(main())
